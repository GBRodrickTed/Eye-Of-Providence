// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/CoolShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _Cam1 ("Texture", 2D) = "white" {}
        _Cam2 ("Texture", 2D) = "white" {}
        _Cam3 ("Texture", 2D) = "white" {}
        _Cam4 ("Texture", 2D) = "white" {}
        _Cam5 ("Texture", 2D) = "white" {}
        _Cam6 ("Texture", 2D) = "white" {}

        _Map ("Texture", 2D) = "white" {}
        _Grid ("Texture", 2D) = "white" {}

        _FOV ("Field of View", Float) = 360
        _MODE ("Projection Mode", Int) = 0
        _ASPECT ("Aspect Ratio", Float) = 1
        _STRETCH ("Stretch to Fit", Float) = 1

        _FISHEYE_EQUIDIST_FIT ("Fisheye Equidist Fit", Float) = 0
        _PANINI_FACTOR ("Panini Factor", Float) = 1
        _WINKEL_FIT ("Winkel Fit", Float) = 0

        _GLOBE_SHIFT_X ("Globe Shift X", Float) = 0
        _GLOBE_SHIFT_Y ("Globe Shift Y", Float) = 0
        _GLOBE_SHIFT_Z ("Globe Shift Z", Float) = 0
        
        _GLOBE_ROTATE ("Globe Rotate", Float) = 0

        _GRID ("Grid View", Int) = 0
        _MAP ("Map View", Int) = 0
        _DEBUG ("Debug Mode", Int) = 0
        _RAY_METHOD ("Ray Method", Int) = 0
        _GRID_FACTOR ("Grid Factor", Float) = 0
        _MAP_FACTOR ("Map Factor", Float) = 0
        
        
    }
    SubShader
    {
        // No culling or depth
        //Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #include "UnityCG.cginc"

            #pragma vertex vert
            #pragma fragment frag

            uniform sampler2D _MainTex;

            uniform sampler2D _Cam1;
            uniform sampler2D _Cam2;
            uniform sampler2D _Cam3;
            uniform sampler2D _Cam4;
            uniform sampler2D _Cam5;
            uniform sampler2D _Cam6;

            uniform float3 _Cam1_Info[4];
            uniform float3 _Cam2_Info[4];
            uniform float3 _Cam3_Info[4];
            uniform float3 _Cam4_Info[4];
            uniform float3 _Cam5_Info[4];
            uniform float3 _Cam6_Info[4];

            uniform sampler2D _Map;
            uniform sampler2D _Grid;

            uniform float _FOV;
            uniform float _MODE;
            uniform float _ASPECT;
            uniform float _STRETCH;

            uniform float _FISHEYE_EQUIDIST_FIT;
            uniform float _PANINI_FACTOR;
            uniform float _WINKEL_FIT;

            uniform float _GLOBE_SHIFT_X;
            uniform float _GLOBE_SHIFT_Y;
            uniform float _GLOBE_SHIFT_Z;

            uniform float _GLOBE_ROTATE;

            uniform int _DEBUG;
            uniform int _GRID;
            uniform int _MAP;
            uniform int _RAY_METHOD;
            uniform float _GRID_FACTOR;
            uniform float _MAP_FACTOR;


            uniform float4 _MainTex_ST;

            uniform float4x4 _viewToWorld;

            uniform float4x4 _rotMatrix;

            uniform sampler2D _CameraDepthNormalsTexture;

            #define M_PI 3.14159265359
            #define M_HALFPI M_PI*0.5
            #define M_E 2.71828
            #define DEG_TO_RAD M_PI/180
            #define DEBUG 1
            #define EQUIRECT 0
            #define FISHEYE 1
            #define STEREO 2
            #define HAMMER 3
            #define PANINI 4
            #define WINKEL 5
            #define QUINCUNCIAL 6
            #define ZEROISH 0.0

            #define RAY_GENERAL 0
            #define RAY_CHEAP 1
            

            int cull = 0;
            float2 globeUV = float2(0,0);
            int globeIndex = 0;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 scrPos : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.scrPos = ComputeScreenPos(o.vertex);
                return o;
            }

            float maptouv(float num)
            {
                return (num+1)*0.5;
            }

            float abs(float num)
            {
                return num * lerp(-1, 1, step(0, num));
            }

            // Current ray-to-globe implementation. Flexable but a bit more pricey. Made with readablility in mind
            // Intermediary variables (planePos/planeDir) can be replaces with it's raw value.
            // If statements should be replaced at some point
            fixed4 rayPlaneIntersectTest(float3 ray)
            {
                float3 planePos; // _Cam_Info[0]
                float3 truePlanePos; 
                static float3 rayPos = float3(0, 0, 0);
                float3 planeDir; // -_Cam_Info[1]
                float rayPlaneDot;
                float rayLength = 1000000;
                float rayLenTest = 0;
                float3 right;
                float3 up;
                int cam = 0;

                // Cam1 test
                planePos = _Cam1_Info[0];
                planeDir = -_Cam1_Info[1];
                rayPlaneDot = dot(ray, planeDir);
                rayLenTest = dot(planePos-rayPos, planeDir)/rayPlaneDot;
                if (rayLenTest < rayLength && rayLenTest > 0)
                {
                    rayLength = rayLenTest;
                    cam = 1;
                    right = _Cam1_Info[3];
                    up = _Cam1_Info[2];
                    truePlanePos = planePos;
                }

                // Cam2 test
                planePos = _Cam2_Info[0];
                planeDir = -_Cam2_Info[1];
                rayPlaneDot = dot(ray, planeDir);
                rayLenTest = dot(planePos-rayPos, planeDir)/rayPlaneDot;
                if (rayLenTest < rayLength && rayLenTest > 0)
                {
                    rayLength = rayLenTest;
                    cam = 2;
                    right = _Cam2_Info[3];
                    up = _Cam2_Info[2];
                    truePlanePos = planePos;
                }

                // Cam3 test
                planePos = _Cam3_Info[0];
                planeDir = -_Cam3_Info[1];
                rayPlaneDot = dot(ray, planeDir);
                rayLenTest = dot(planePos-rayPos, planeDir)/rayPlaneDot;
                if (rayLenTest < rayLength && rayLenTest > 0)
                {
                    rayLength = rayLenTest;
                    cam = 3;
                    right = _Cam3_Info[3];
                    up = _Cam3_Info[2];
                    truePlanePos = planePos;
                }

                // Cam4 test
                planePos = _Cam4_Info[0];
                planeDir = -_Cam4_Info[1];
                rayPlaneDot = dot(ray, planeDir);
                rayLenTest = dot(planePos-rayPos, planeDir)/rayPlaneDot;
                if (rayLenTest < rayLength && rayLenTest > 0)
                {
                    rayLength = rayLenTest;
                    cam = 4;
                    right = _Cam4_Info[3];
                    up = _Cam4_Info[2];
                    truePlanePos = planePos;
                }

                // Cam5 test
                planePos = _Cam5_Info[0];
                planeDir = -_Cam5_Info[1];
                rayPlaneDot = dot(ray, planeDir);
                rayLenTest = dot(planePos-rayPos, planeDir)/rayPlaneDot;
                if (rayLenTest < rayLength && rayLenTest > 0)
                {
                    rayLength = rayLenTest;
                    cam = 5;
                    right = _Cam5_Info[3];
                    up = _Cam5_Info[2];
                    truePlanePos = planePos;
                }

                // Cam6 test
                planePos = _Cam6_Info[0];
                planeDir = -_Cam6_Info[1];
                rayPlaneDot = dot(ray, planeDir);
                rayLenTest = dot(planePos-rayPos, planeDir)/rayPlaneDot;
                if (rayLenTest < rayLength && rayLenTest > 0)
                {
                    rayLength = rayLenTest;
                    cam = 6;
                    right = _Cam6_Info[3];
                    up = _Cam6_Info[2];
                    truePlanePos = planePos;
                }

                float3 rayIntersectPos = rayPos+ray*rayLength - truePlanePos;
                
                // This is 2 by 2
                float x = dot(right, rayIntersectPos);
                float y = dot(up, rayIntersectPos);

                /*
                //https://math.stackexchange.com/questions/180418/calculate-rotation-matrix-to-align-vector-a-to-vector-b-in-3d
                //planeDir = ray;
                static float3x3 identity = float3x3(1, 0, 0, 0, 1, 0, 0, 0, 1);
                float3 v = cross(planeDir, normDir);
                float3x3 skew = float3x3(0, -v.z, v.y, v.z, 0, -v.x, -v.y, v.x, 0);
                float c = dot(planeDir, normDir);
                float3x3 rotMatrix = identity + skew + mul(skew, skew)*(1/(1+c));

                float3 testRay = mul(ray, rotMatrix);*/
                //return float4(testRay.x, testRay.y, testRay.z, 1);

                //rayIntersectPos = mul(rayIntersectPos, rotMatrix);

               float4 color = float4(0, 0, 0, 0);
               //return color;

               float2 uv = float2(x,y)*0.5 + 0.5;
               globeIndex = cam - 1;
               globeUV = uv;

               //return float4(uv.x, uv.y, 0, 1);

               color += tex2D(_Cam1, uv) * step(cam, 1.5);
               
               color += tex2D(_Cam2, uv) * step(cam, 2.5) * step(1.5, cam);
               color += tex2D(_Cam3, uv) * step(cam, 3.5) * step(2.5, cam);
               color += tex2D(_Cam4, uv) * step(cam, 4.5) * step(3.5, cam);
               color += tex2D(_Cam5, uv) * step(cam, 5.5) * step(4.5, cam);
               color += tex2D(_Cam6, uv) * step(cam, 6.5) * step(5.5, cam);

               color *= step(abs(x), 1);
               color *= step(abs(y), 1);
               //color += tex2D(_Cam6, uv) * step(cam, 6.5) * step(5.5, cam);

               return color;

               
            }

            // First iteration of the ray-to-globe system. Cheaper but rigid. All if statements should be replaced with lerp/steps()'s
            fixed4 mapRayToGlobe(float3 ray)
            {
                float4 debugColor = float4(1, 1, 1, 1);
                if (abs(ray.x/ray.z) < 1 && abs(ray.y/ray.z) < 1 && ray.z > 0)// Front
                {
                    float2 uv = float2(ray.x/ray.z*0.5, ray.y/ray.z*0.5);
                    uv += 0.5;
                    globeUV = uv;
                    globeIndex = 0;
                    return tex2D(_Cam1, uv);
                } else if (abs(ray.x/ray.z) < 1 && abs(ray.y/ray.z) < 1 && ray.z < 0) // Back
                { 
                    float2 uv = float2(ray.x/ray.z*0.5, ray.y/ray.z*-0.5);
                    uv += 0.5;
                    globeUV = uv;
                    globeIndex = 1;
                    return tex2D(_Cam2, uv);
                } else if (abs(ray.z/ray.x) < 1 && abs(ray.y/ray.x) < 1 && ray.x < 0) // Left
                {
                    //return float4(uv, 0, 0);
                    float2 uv = float2(ray.z/ray.x*-0.5, ray.y/ray.x*-0.5);
                    uv += 0.5;
                    globeUV = uv;
                    globeIndex = 2;
                    return tex2D(_Cam3, uv);
                } else if (abs(ray.z/ray.x) < 1 && abs(ray.y/ray.x) < 1 && ray.x > 0) // Right
                {
                    float2 uv = float2(ray.z/ray.x*-0.5, ray.y/ray.x*0.5);
                    uv += 0.5;
                    globeUV = uv;
                    globeIndex = 3;
                    return tex2D(_Cam4, uv);
                } else if (abs(ray.x/ray.y) < 1 && abs(ray.z/ray.y) < 1 && ray.y < 0) // Down
                {
                    float2 uv = float2(ray.x/ray.y*-0.5, ray.z/ray.y*-0.5);
                    uv += 0.5;
                    globeUV = uv;
                    globeIndex = 4;
                    return tex2D(_Cam6, uv);
                } else if (abs(ray.x/ray.y) < 1 && abs(ray.z/ray.y) < 1 && ray.y > 0) // Up
                {
                    float2 uv = float2(ray.x/ray.y*0.5, ray.z/ray.y*-0.5);
                    uv += 0.5;
                    globeUV = uv;
                    globeIndex = 5;
                    return tex2D(_Cam5, uv);
                } else {
                    //return float4(0,0,0,0);
                }

                

                return float4(abs(ray.x), abs(ray.y), abs(ray.z), 0);
            }

            //Based on https://en.wikipedia.org/wiki/Equirectangular_projection
            float2 equirect(float2 normalCoords)
            {
                float fov = _FOV*DEG_TO_RAD;
                if (!_STRETCH)
                {
                    normalCoords = float2(normalCoords.x, normalCoords.y * (2/_ASPECT));
                }

                float2 angleCoords = normalCoords * (fov*0.5);

                float lat = angleCoords.x + 0;
                float lon = angleCoords.y * 0.5;

                if (abs(lon) > M_HALFPI) {
                    cull = 1;
                }
                
                return float2(lat, lon);
            }

            // Based on straight thuggin
            float2 fisheye_equidist(float2 normalCoords)
            {
                float fov = _FOV*DEG_TO_RAD;
                float scale = _FISHEYE_EQUIDIST_FIT;
                float aspect = _ASPECT;
                if (_STRETCH == 1)
                {
                    aspect = 1;
                }
                float stick = lerp(1,sqrt(aspect * aspect + 1), scale);
                normalCoords = float2(normalCoords.x * aspect, normalCoords.y);
                if (length(normalCoords) < stick)
                {
                    float r = length(normalCoords);
                    
                    float angle = atan2(normalCoords.y, normalCoords.x);
                    
                    float factor = fov/(360*DEG_TO_RAD)*(1/stick);

                    float nr = r * factor * 2;

                    float2 nuv = float2((angle), (nr-1)*(-90*DEG_TO_RAD));
                    
                    float2 angleCoords = nuv;
                    
                    return angleCoords;
                } else {
                    cull = 1;
                    return float2(0, 0);
                }
            }

            // Useful source https://mathworld.wolfram.com/StereographicProjection.html
            float2 fisheye_stereo(float2 normalCoords)
            {
                float fov = _FOV*DEG_TO_RAD;

                float r = 1;
                float factor = 2*r*tan(fov*0.25);
                //float new_factor = 2*r*tan(fov*0.25);

                float2 planecoord = normalCoords * factor;
                if (!_STRETCH)
                {
                    planecoord = float2(planecoord.x, planecoord.y/(_ASPECT));
                }
                float p = length(planecoord); // length of pixel away from center. If thought in terms of trig, this is opposite and 2r (diameter of sphere touching the pixel plane) is adjacent
                float c = 2*atan(p/(2 * r)); // view longitude
                float clon = 0;
                float clat = 0;

                float sinc = sin(c);
                float cosc = cos(c);
                float sinclat = 0;//sin(clat);
                float cosclat = 1;//cos(clat);

                float x = planecoord.x;
                float y = planecoord.y;
                
                float lon = clon + atan((x*sinc)/(p*cosclat*cosc-y*sinclat*sinc));
                float lat = asin(cosc*sinclat+(y*sinc*cosclat)/p);
                
                if (c >= 90*DEG_TO_RAD)
                {
                    lon -= 180*DEG_TO_RAD;
                }
                return float2(lon, lat);
            }

            // based on https://github.com/shaunlebron/blinky/blob/master/game/lua-scripts/lenses/hammer.lua
            float2 hammer(float2 normalCoords)
            {
                float2 planecoord = normalCoords;
                //planecoord.x *= 2;//
                if (!_STRETCH)
                {
                    planecoord = float2(planecoord.x, planecoord.y * (2/_ASPECT));
                }
                float fov = (_FOV) / (360);
                float view = 2*sqrt(2) * fov; // Brute forced, should figure out why this is the case
                float x = planecoord.x * view * 1;
                float y = planecoord.y * view * 0.5;

                if (x*x/8+y*y/2 > 1)
                {
                    cull = 1;
                    return float2(0, 0);
                }
                float z = sqrt(1-0.0625*x*x-0.25*y*y);//sqrt(1-((0.25*x)*(0.25*x))-((0.5*y)*(0.5*y)));
                float lon = 2*atan(z*x/(2*(2*z*z-1)));//2*atan((z*x)/(2*((2*z*z)-1)));
                float lat = asin(z*y);//asin(z*y);
                //if (p > view)
                //{
                    //cull = 1;
                //}
                return float2(lon, lat);
            }
            // base on https://en.wikipedia.org/wiki/Hammer_projection
            float2 ahammer(float2 normalCoords)
            {
                float2 planecoord = normalCoords;
                //planecoord.x *= 2;//
                if (!_STRETCH)
                {
                    planecoord = float2(planecoord.x, planecoord.y * (2/_ASPECT));
                }
                float fov = (_FOV * DEG_TO_RAD) / (2*M_PI);
                float view = sqrt(2)*2; // Brute forced, should figure out why this is the case
                float x = planecoord.x * view * 1 * fov;
                float y = planecoord.y * view * 0.5 * fov;
                float p = sqrt(x*x + y*y*4); // also brute forced
                float z = sqrt(1-((0.25*x)*(0.25*x))-((0.5*y)*(0.5*y)));
                float lon = 2*atan((z*x)/(2*((2*z*z)-1)));
                float lat = asin(z*y);
                if (p > view)
                {
                    cull = 1;
                }
                return float2(lon, lat);
            }

            float2 panini(float2 normalCoords) // http://tksharpless.net/vedutismo/Pannini/panini.pdf
            {
                if (!_STRETCH)
                {
                    normalCoords = float2(normalCoords.x, normalCoords.y/(_ASPECT) );
                }
                
                // calculates fov
                // taken from on https://www.shadertoy.com/view/Wt3fzB
                // fovs greater than or less than 180 don't seem to be entirely accurate

                float fov = (_FOV * DEG_TO_RAD);

                float d = _PANINI_FACTOR;

                float d2 = d*d;

                float fo = M_PI*0.5 - fov * 0.5;

                float f = cos(fo)/sin(fo) * 2.0;
                float f2 = f*f;

                float b = (sqrt(max(0.0, pow(d+d2, 2)*(f2+f2*f2))) - (d*f+f)) / (d2+d2*f2-1.0);

                normalCoords *= b;

                // naturally 180 by 45
                float h = normalCoords.x;
                float v = normalCoords.y;
                float k = (h*h)/((d+1)*(d+1));
                float discrm = (k*k*d*d)-((k+1)*((k*d*d)-1));
                float cosAprox = ((-k*d)+sqrt(discrm))/(k+1);
                float s = (d+1)/(d+cosAprox);
                float lat = atan2(h,s*cosAprox);
                float lon = atan2(v,s);
                return float2(lat, lon);
            }

            // unrelated to the implementation but this is an intresting find https://www.boehmwanderkarten.de/kartographie/is_netze_winkel_tripel_inversion.html
            // Based on https://github.com/shaunlebron/blinky/blob/master/game/lua-scripts/lenses/winkeltripel.lua
            float2 winkeltripel(float2 normalCoords)
            {
                float2 planecoord = normalCoords;
                //planecoord.x *= 2;//
                if (!_STRETCH)
                {
                    planecoord = float2(planecoord.x, planecoord.y * (_ASPECT));
                }
                //planecoord *= 4.574; // Brute Forced
                planecoord *= M_PI;
                if (!_STRETCH)
                {
                    planecoord *= 0.82; // Brute Forced
                }

                if (!_STRETCH)
                {
                    planecoord.y /= lerp(1, 0.82*(_ASPECT), _WINKEL_FIT);
                    //planecoord.y /= lerp(1, (_ASPECT), _WINKEL_FIT);
                } else {
                    planecoord.x *= lerp(1, 0.82, _WINKEL_FIT);
                }

                planecoord *= _FOV/360;
                
                float x = planecoord.x;
                float y = planecoord.y * 0.5;

                float lambda = x; // lambda lon
                float phi = y; // phi lat
                float eps = 0.0001;
                float halfpi = M_PI/2;
                float skip = 0;

                float lens_height = M_PI/2;
                if (abs(y) > lens_height)
                {
                    cull = 1;
                    return float2(0, 0);
                }


                
                float cosphi = 0;
                float sinphi = 0;
                float cos2phi = 0;
                float sinlambda_2 = 0;
                float sin2lambda_2 = 0;
                float coslambda_2 = 0;
                float sin2phi = 0;
                float sin_2phi = 0;
                float sinlambda = 0;
                float E = 0;
                float F = 0;
                
                [unroll(5)]
                for (int i = 0; i < 5; i++)
                {
                    if (skip < 0.5)
                    {
                        cosphi = cos(phi);
                        sinphi = sin(phi);
                        sin_2phi = sin(2 * phi);
                        sin2phi = sinphi * sinphi;
                        cos2phi = cosphi * cosphi;
                        sinlambda = sin(lambda);
                        coslambda_2 = cos(lambda / 2);
                        sinlambda_2 = sin(lambda / 2);
                        sin2lambda_2 = sinlambda_2 * sinlambda_2;
                        float C = 1 - cos2phi * coslambda_2 * coslambda_2;
                        

                        E = 0;
                        F = 0;

                        if (C >= ZEROISH || C <= -ZEROISH)
                        {
                            F = 1/C;
                            E = acos(cosphi * coslambda_2) * sqrt(F);
                        } else {
                            E = 0;
                            F = 0;
                        }
                        

                        float fx = 0.5 * (2 * E * cosphi * sinlambda_2 + lambda / halfpi) - x;
                        float fy = 0.5 * (E * sinphi + phi) - y;
                        float sigxsiglambda = 0.5 * F * (cos2phi * sin2lambda_2 + E * cosphi * coslambda_2 * sin2phi) + 0.5 / halfpi;
                        float sigxsigphi = F * (sinlambda * sin_2phi / 4 - E * sinphi * sinlambda_2);
                        float sigysiglambda = 0.125 * F * (sin_2phi * sinlambda_2 - E * sinphi * cos2phi * sinlambda);
                        float sigysigphi = 0.5 * F * (sin2phi * coslambda_2 + E * sin2lambda_2 * cosphi) + 0.5;
                        float denominator = sigxsigphi * sigysiglambda - sigysigphi * sigxsiglambda;
                        float siglambda = (fy * sigxsigphi - fx * sigysigphi) / denominator;
                        float sigphi = (fx * sigysiglambda - fy * sigxsiglambda) / denominator;
                        lambda = lambda - siglambda;
                        phi = phi - sigphi;
                        if (abs(siglambda) < eps && abs(sigphi) < eps)
                        {
                            skip = 1;
                        }
                    }
                }
                if (abs(lambda) > M_PI || abs(phi) > halfpi)
                {
                    cull = 1;
                }
                
                // Brute forced to cut out artifacts
                if (length(planecoord) > 3.3)
                {
                    cull = 1;
                }


                return float2(lambda, phi);
            }

            // globe projection final boss (under construction)
            // based on https://github.com/shaunlebron/blinky/blob/master/game/lua-scripts/lenses/quincuncial.lua
            
            float2 rotate(float2 ab, float angle)
            {
                float c = cos(angle);
                float s = sin(angle);
                float a0 = ab.x*c - ab.y*s;
                float b0 = ab.x*s + ab.y*c;
                return float2(a0, b0);
            }
            float asqrt(float x)
            {
                if (x > 0)
                {
                    return sqrt(x);
                }
                return 0;
            }
            float cosh(float x)
            {
                return (pow(M_E, x) + pow(M_E, -x))/2;
            }
            float sinh(float x)
            {
                return (pow(M_E, x) - pow(M_E, -x))/2;
            }
            float tanh(float x)
            {
                float e2x = pow(M_E, 2 * x);
                return (e2x - 1)/(e2x + 1);
            }

            //static float a[9] = (1, 0, 0, 0, 0, 0, 0, 0, 0};
            // returns floatt(sn, cn, dn, ph)
            // Based on ALGLIB 4.08.0 c++ implementation https://www.alglib.net/specialfunctions/jacobianelliptic.php
            float4 ellipj(float u, float m)
            {
                float ai;
                float b;
                float phi;
                float t;
                float twon;

                float eps = 0.0001;
                float halfpi = M_PI/2;

                if (m < eps) {
                    t = sin(u);
                    b = cos(u);
                    ai = 0.25 * m * (u - t * b);
                    return float4(
                        t - ai * b, 
                        b + ai * t, 
                        1 - 0.5 * m * t * t, 
                        u - ai
                    );
                }
                if (m >= (1 - eps)) {
                    t = tanh(u);
                    b = cosh(u);
                    ai = 0.25 * (1 - m);
                    phi = 1 / b;//
                    twon = b * sinh(u);
                    return float4(
                        t + ai * (twon - u) / (b * b), 
                        phi - ai * (twon - u), 
                        phi + ai * (twon + u), 
                        2 * atan(exp(u)) - halfpi + ai * (twon - u) / b
                    );
                }
                //return float4(0, 0, 0, 0);
                //float a[9];
                // Couldn't figure out local arrays so i wrote it out by hand like the fricken chad that i am >:)
                float a1 = 1;
                float a2 = 0;
                float a3 = 0;
                float a4 = 0;
                float a5 = 0;
                float a6 = 0;
                float a7 = 0;
                float a8 = 0;
                float a9 = 0;

                float c1 = sqrt(m);
                float c2 = 0;
                float c3 = 0;
                float c4 = 0;
                float c5 = 0;
                float c6 = 0;
                float c7 = 0;
                float c8 = 0;
                float c9 = 0;

                b = sqrt(1 - m);
                twon = 1;

                float iter = 0;

                // do 4 iterations

                if (abs(c1 / a1) > eps)
                {
                    ai = a1;
                    iter = iter + 1;
                    c2 = 0.5 * (ai-b);
                    t = sqrt(ai * b);
                    a2 = 0.5 * (ai+b);
                    b = t;
                    twon = twon*2;
                }

                if (abs(c2 / a2) > eps)
                {
                    ai = a2;
                    iter = iter + 1;
                    c3 = 0.5 * (ai-b);
                    t = sqrt(ai * b);
                    a3 = 0.5 * (ai+b);
                    b = t;
                    twon = twon*2;
                }

                if (abs(c3 / a3) > eps)
                {
                    ai = a3;
                    iter = iter + 1;
                    c4 = 0.5 * (ai-b);
                    t = sqrt(ai * b);
                    a4 = 0.5 * (ai+b);
                    b = t;
                    twon = twon*2;
                }

                if (abs(c4 / a4) > eps)
                {
                    ai = a4;
                    iter = iter + 1;
                    c5 = 0.5 * (ai-b);
                    t = sqrt(ai * b);
                    a5 = 0.5 * (ai+b);
                    b = t;
                    twon = twon*2;
                }

                phi = twon*u;
                float a0 = 0;
                float c0 = 0;
                // kill 2 birds for the price of one by multing phi
                // and setting a0/c0 which is the value of the pseudo array
                if (iter > 3.5) { // 4
                    phi *= a5;
                    a0 = a5;
                } else if (iter > 2.5) // "3"
                {
                    phi *= a4;
                    a0 = a4;
                } else if (iter > 1.5) { // "2"
                    phi *= a3;
                    a0 = a3;
                } else if (iter > 0.5){ // "1"
                    phi *= a2;
                    a0 = a2;
                } else { // "0"
                    phi *= a1;
                    a0 = a1;
                }

                
                // one iteration is done unconditially then check the rest
                t = c0 * sin(phi)/ a0;
                b = phi;
                phi = (asin(t) + phi)/2;
                iter = iter - 1;

                if (iter > 0.5)
                {
                    if (iter > 3.5)
                    {
                        a0 = a5;
                    } else if (iter > 2.5) {
                        a0 = a4;
                    } else if (iter > 1.5){
                        a0 = a3;
                    } else if (iter > 0.5){
                        a0 = a2;
                    } else {
                        a0 = a1;
                    }
                    t = c0 * sin(phi)/ a0;
                    b = phi;
                    phi = (asin(t) + phi)/2;
                    iter = iter - 1;
                }

                if (iter > 0.5)
                {
                    if (iter > 3.5)
                    {
                        a0 = a5;
                    } else if (iter > 2.5) {
                        a0 = a4;
                    } else if (iter > 1.5){
                        a0 = a3;
                    } else if (iter > 0.5){
                        a0 = a2;
                    } else {
                        a0 = a1;
                    }
                    t = c0 * sin(phi)/ a0;
                    b = phi;
                    phi = (asin(t) + phi)/2;
                    iter = iter - 1;
                }

                

                if (iter > 0.5)
                {
                    if (iter > 3.5)
                    {
                        a0 = a5;
                    } else if (iter > 2.5) {
                        a0 = a4;
                    } else if (iter > 1.5){
                        a0 = a3;
                    } else if (iter > 0.5){
                        a0 = a2;
                    } else {
                        a0 = a1;
                    }
                    t = c0 * sin(phi)/ a0;
                    b = phi;
                    phi = (asin(t) + phi)/2;
                    iter = iter - 1;
                }
                
                t = cos(phi);
                return float4(sin(phi), t, t/cos(phi-b), phi);
            }

            float2 cnrectify(float x, float y)
            {
                float sqrt2 = sqrt(2)*_FOV/360;
                float sqrt22 = sqrt2/2;
                float m = 0.5;
                float ke = 1.85407467730137;

                float eps = 0.0001;
                float halfpi = M_PI/2;
                
                float xpr = ke*(sqrt22*x-sqrt22*y)/sqrt2+ke;
                float ypr = ke*(sqrt22*x+sqrt22*y)/sqrt2;
                float sni;
                float cni;
                float dni;
                float x1;
                float y1;
                float phi;
                float psi;
                float s;
                float c;
                float d;
                float s1;
                float c1;
                float d1;
                float delta;

                if (abs(ypr) < eps)
                {
                    float4 ni = ellipj(xpr, m);
                    sni = ni.x;
                    cni = ni.y;
                    dni = ni.z;
                    x1 = cni;
                    y1 = 0;
                } else {
                    phi = xpr;
                    psi = ypr;

                    float4 elj = ellipj(phi, m);
                    s = elj.x;
                    c = elj.y;
                    d = elj.z;

                    float4 elj1 = ellipj(psi, 1-m);
                    s1 = elj1.x;
                    c1 = elj1.y;
                    d1 = elj1.z;
                    
                    delta = pow(c1,2) + pow(m*s,2)*pow(s1,2);

                    x1 = (c*c1)/delta;
                    y1 = -(s*d*s1*d1)/delta;
                }

                float lond = atan2(y1,x1);
                float latp = 2*atan2(sqrt(x1*x1+y1*y1), 1)-halfpi;
                
                return float2(lond, latp);
            }

            float2 quincuncial(float2 normalCoords)
            {
                float sqrt2 = sqrt(2);
                
                
                float x = normalCoords.x * 2;
                float y = normalCoords.y * 2;
                float2 xy0 = float2(x,y);

                float2 latlon = cnrectify(xy0.x, xy0.y);
                return latlon;
                float rot = 0;

                // Front
                if (abs(x) + abs(y) < sqrt2)
                {
                    xy0 = rotate(float2(x,y), rot);
                    xy0.x -= 1;
                } else if (x > 0 && y < 0)
                {
                    xy0 = rotate(float2(x,y), rot);
                    xy0.x -= 1;
                } else if (x < 0 && y > 0)
                {
                    xy0 = rotate(float2(x,y), rot);
                    xy0.x += 3;
                } else if (x < 0 && y < 0)
                {
                    xy0 = rotate(float2(x,y), rot+M_PI);
                    xy0.x += 1;
                    xy0.y -= 2;
                } else {
                    xy0 = rotate(float2(x,y), rot+M_PI);
                    xy0.x += 1;
                    xy0.y += 2;
                }
                xy0.x += 1;
                latlon = cnrectify(xy0.x, xy0.y);
                return latlon;
            }
            
            

            float4 debugColor(int index)
            {
                switch(index)
                {
                    case 0: // front
                        return float4(1, 1, 1, 1);
                        break;
                    case 1: // back
                        return float4(1, 0, 1, 1);
                        break;
                    case 2: // left
                        return float4(1, 1, 0, 1);
                        break;
                    case 3: //right
                        return float4(0, 1, 0, 1);
                        break;
                    case 4: // up
                        return float4(0, 0, 1, 1);
                        break;
                    case 5: // down
                        return float4(1, 0, 0, 1);
                        break;
                }
                return float4(1, 1, 1, 1);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //return tex2D(_Cam1, i.uv);
                //return float4(1, 0, 1, 0);
                float2 normalCoords = (i.uv-0.5) * 2;
                float2 geoCoords = float2(0,0);

                switch(_MODE)
                {
                    case EQUIRECT:
                        geoCoords = equirect(normalCoords);
                        break;
                    case FISHEYE:
                        geoCoords = fisheye_equidist(normalCoords);
                        break;
                    case STEREO:
                        geoCoords = fisheye_stereo(normalCoords);
                        break;
                    case HAMMER:
                        geoCoords = hammer(normalCoords);
                        break;
                    case PANINI:
                        geoCoords = panini(normalCoords);
                        break;
                    case WINKEL:
                        geoCoords = winkeltripel(normalCoords);
                        break;
                    case QUINCUNCIAL:
                        geoCoords = quincuncial(normalCoords);
                        break;
                    default:
                        geoCoords = equirect(normalCoords);
                        break;
                }
                //geoCoords = quincuncial(normalCoords);
                //cull = 0;
                float lat = geoCoords.x;
                float lon = geoCoords.y;

                

                //float2 newt = wrinkleRemapped(float2(lat, lon)) * 2.5;

                float x = cos(lon) * sin(lat);
                float y = sin(lon);
                float z = cos(lon) * cos(lat);

                float3 poleRay = float3(
                    x, // x
                    y, // y
                    z  // z
                );

                // This if statment is actually ok cause it effects all pixels :p
                if (_MODE == FISHEYE)
                {
                    poleRay = float3(
                        z, // x
                        x, // y
                        y // z
                    );
                }

                
                

                if (_GLOBE_ROTATE > 0.5)
                {
                    poleRay = mul(_rotMatrix, poleRay);
                    //poleRay = mul(_yaw, poleRay);
                    //poleRay = mul(_pitch, poleRay);
                    //poleRay = mul(_roll, poleRay);
                }

                //return float4(_yaw, poleRay.y, poleRay.z, 1);
                //if (ellipj(normalCoords.y, 1))
                

                if (cull == 0)
                {
                    //return float4(geoCoords, 0, 0);
                    float4 globe; //rayPlaneIntersectTest(poleRay);
                    switch (_RAY_METHOD)
                    {
                        case RAY_CHEAP:
                            globe = mapRayToGlobe(poleRay);
                            break;
                        case RAY_GENERAL:
                            globe = rayPlaneIntersectTest(poleRay);
                            break;
                        default:
                            globe = rayPlaneIntersectTest(poleRay);
                            break;
                    }
                    
                    if (_GRID == 1)
                    {
                        float4 grid = tex2D(_Grid, globeUV);
                        grid *= debugColor(globeIndex);
                        //return grid;
                        globe = float4(lerp(globe.xyz, grid.xyz, grid.a * _GRID_FACTOR), 1);
                        //return float4(0, 0, 0, 0);
                        //return globe;
                    }
                    if (_MAP == 1) {
                        float2 mapUV = float2(geoCoords.x/(180*DEG_TO_RAD), geoCoords.y/(90*DEG_TO_RAD));
                        mapUV /= 2;
                        mapUV.y *= 1;
                        mapUV += 0.5;
                        float4 map = tex2D(_Map, mapUV);
                        globe = float4(lerp(globe.xyz, map.xyz, map.a * _MAP_FACTOR), 1);
                        //return float4(0, 0, 0, 0);
                    }
                    return globe;
                } else {
                    return float4(0, 0, 0, 0);
                }
                

                //float2 angleCoords = normalCoords * fov;

                //float fisheye = _FISHEYE;
                
                // if (fisheye > 0.5)
                // {
                //     float scale = _FISHEYE_FIT;
                //     float stick = lerp(1,sqrt(_ASPECT * _ASPECT + 1), scale);
                //     normalCoords = float2(normalCoords.x * _ASPECT, normalCoords.y);
                //     angleCoords = normalCoords * fov;
                //     float2 tempCoords = normalCoords*2;
                //     if (length(tempCoords) < stick)
                //     {
                //         //float2 tempCoords = normalCoords*2;
                //         float2 dir = float2(1, 0);
                //         float r = length(tempCoords);

                //         float num = 2 * tan(r*fov/2);
                //         //r = num / (2*tan(fov/2));
                        
                //         //return float4(i.uv, 0, 0);
                        
                //         float angle = atan2(tempCoords.y, tempCoords.x);
                        
                //         float factor = fov/(360*DEG_TO_RAD)*(1/stick);

                //         float nr = r * factor * 2;

                //         float2 nuv = float2(angle/(M_PI), nr-1) * 0.5;
                        
                //         angleCoords = (nuv) * 360*DEG_TO_RAD;
                //         //angleCoords.x = nuv.x * 360*DEG_TO_RAD; 
                //         //angleCoords.y = nuv.y * fov; 
                //         //return tex2D(_Cam_Front, nuv);
                //         //return float4(normalCoords, 0, 0);
                //         //return float4((nuv), 0, 0);
                //         //return float4((nuv-0.5), 0, 0);
                //         //return float4(normalCoords, 0, 0);
                //     } else {
                //         return float4(0, 0, 0, 0);
                //     }
                // } else {
                //     if (_EQUIRECT_STRETCH < 0.5)
                //     {
                //         normalCoords = (i.uv-0.5);
                //         normalCoords = float2(normalCoords.x, normalCoords.y * (2/_ASPECT));
                //         angleCoords = normalCoords * fov;

                //         if (abs(normalCoords.x) > 0.5 || abs(normalCoords.y) > 0.5) {
                //             return float4(0, 0, 0, 0);
                //         }
                //     }
                // }
                
                // // maps uvs to the angles of the rays being shot. x is lat, y is lon

                // float3 pole = float3(0,0,-1);

                // float lat = angleCoords.x + 0*DEG_TO_RAD;
                // float lon = angleCoords.y * 0.5 - 0*DEG_TO_RAD;

                

                // float x = cos(lon) * sin(lat);
                // float y = sin(lon);
                // float z = cos(lon) * cos(lat);

                // float3 poleRay = float3(
                //     x, // x
                //     y, // y
                //     z  // z
                // );

                // if (fisheye > 0.5)
                // {
                //     poleRay = float3(z, x, -y);
                // }
                // //return float4(abs(x), abs(y), abs(z), 0);
                // float r = 1;
                // float2 planecoord = normalCoords * _FOV * DEG_TO_RAD; 
                // planecoord = float2(planecoord.x * (_ASPECT), planecoord.y);
                // float p = length(planecoord); // length of pixel away from center. If thought in terms of trig, this is opposite and 2r (diameter of sphere touching the pixel plane) is adjacent
                // float c = 2*atan(p/(2 * r)); // view longitude
                // float clon = 0;
                // float clat = 0;
                

                // x = planecoord.x;
                // y = planecoord.y;
                
                // lon = clon + atan((x*sin(c))/(p*cos(clat)*cos(c)-y*sin(clat)*sin(c)));
                // lat = asin(cos(c)*sin(clat)+(y*sin(c)*cos(clat))/p);

                // //lon += _FOV * DEG_TO_RAD;

                // //float l = 2 * r * cos(lat);
                // if (c > 90*DEG_TO_RAD)
                // {
                //     lon += 180*DEG_TO_RAD;
                // }


                // float2 newUV = float2(lon, lat);

                // newUV.x /= 360*DEG_TO_RAD;
                // newUV.y /= 180*DEG_TO_RAD;
                // newUV += 0.5;
                // return tex2D(_Map, newUV);
                // return float4(newUV-0.5, 0, 0);
                // // x = cos(lon) * sin(lat);
                // // y = sin(lon);
                // // z = cos(lon) * cos(lat);
                
                // //return float4(newUV, 0, 0);

                // x = cos(lat) * sin(lon);
                // y = sin(lat);
                // z = cos(lat) * cos(lon);

                // poleRay = float3(
                //     x, // x
                //     y, // y
                //     z  // z
                // );

                // //float3 newRay = normalize(poleRay) * l;
                // //newRay.z -= 1;

                // //return tex2D(_Cam_Front, newUV);
                // //return float4(newUV, 0, 0);
                
                // return mapRayToGlobe(poleRay);

                // //float2 normUV = float2(abs(normalCoords.x)*2, abs(normalCoords.y)*2);
                // //float2 newUV = float2(normUV.x * cos(normUV.y*0.5*M_PI), normUV.y);
                // //newUV.x *= cos(newUV.y)
                // if (length(normalCoords*2) > 1)
                // {
                //     //return float4(0, 0, 0, 0);
                // }
                // //return float4(newUV.x, 0 , 0, 0);
                // //return tex2D(_MainTex, newUV);

                // if (length(normalCoords*2) > 1)
                // {
                //     //return float4(0, 0, 0, 0);
                // }

                

                /*if ((lat < DEG * 45 && lat > -DEG * 45) && (lon < DEG * 45 && lon > -DEG * 45))
                {
                    return float4(1, 0, 0, 0);
                } else {
                    return float4(0, 0, 0, 0);
                }*/
                
                

                //return float4(i.uv.x, i.uv.y, 0, 0);
                //return float4(x, y, z, 0);
                //return float4(abs(x), abs(y), abs(z), 0);
                //return float4(abs(x), abs(y), abs(z), 0);
                // Equirectangular Projection (Panoramic)
                //float3 poleRay = float3(
                //    cos(lon) * sin(lat), // x
                //    sin(lon),            // y
                //    cos(lon) * cos(lat)  // z
                //);
                //return equirectangularProg(ray);
                //return camCol2;
            }

            
            ENDCG
        }
    }
}