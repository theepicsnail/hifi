/*
{
  "ProceduralEntity": {
    "shaderUrl": "http://home.snail.rocks:8000/shaders/things.fs",
    "version": 2,
    "uniforms": {},
    "channels": []
  }
}
*/
#define PI 3.14159265358
#define MAX_DIST 10.0
#define EPS 0.001
#define ITR 100.0
#define iTime iGlobalTime

vec3 lastCell = vec3(0);

vec2 rotate(vec2 v, float angle) {return cos(angle)*v+sin(angle)*vec2(v.y,-v.x);}

float rand(vec2 co){
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}

vec2 rand2(vec2 co){
    return fract(sin(vec2(dot(co.xy ,vec2(12.9898,78.233)),dot(co.yx,vec2(13.1898,73.231)))) * 43758.5453);
}

mat3 lookat(vec3 fw){
	fw=normalize(fw);vec3 rt=normalize(cross(fw,vec3(0.0,1.0,0.0)));return mat3(rt,cross(rt,fw),fw);
}


vec3 hsv2rgb(vec3 c)
{
    vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}
float SDF(vec3 ro, vec3 rd) {
    float d =MAX_DIST;
    
    float angle = sin(iTime)*.0005+iTime*.01;
    for(int i = 0 ; i < 4 ; i++){
        angle += float(i);
        ro.x += 1;
        ro.xy = rotate(ro.xy,angle);
        angle += float(i);
    	ro.yz = rotate(ro.yz,angle);
        angle += float(i);
    	ro.zx = rotate(ro.zx,angle);
    	ro.y = abs(ro.y);
    	//ro.yz = rotate(ro.yz,-angle);
    	//ro.xy = rotate(ro.xy,-angle);
    }
    
    vec3 c = ro+.5;
    lastCell = floor(c);
    ro=fract(c)-.5;
    
    float t= min(.01,length(ro)-.2);
    d = min(d, length(ro)*.1);
    d = min(d, length(ro.xy)-t);
    d = min(d, length(ro.yz)-t);
    d = min(d, length(ro.zx)-t);
    return d;
}


vec3 scene(vec3 ro, vec3 rd) {
    vec3 p = ro;
    float t;
    float d;
    float i = 0.0;
    float c = 1.0;
    for(; i < ITR ; i++) {
    	t += d = SDF(ro+rd*t,rd);
        c = min(d,c);
        if(t > MAX_DIST || d < EPS) break;
    }

    c = SDF(ro+rd*t,rd);
    vec2 delta = vec2(.0001,0);
    float cx = SDF(ro+rd*t+delta.xyy, rd);
    float cy = SDF(ro+rd*t+delta.yxy, rd);
    float cz = SDF(ro+rd*t+delta.yyx, rd);
    vec3 dir = normalize(vec3(
        cx-c,
        cy-c,
        cz-c
    ));
    float light = dot(rd, dir);
    
    return dir * (i < ITR ? 1.0:0.0);

    return hsv2rgb(vec3(
        sin(length(lastCell)*100.0)*.1 + 0*t*sin(iTime*.2)*.5 + iTime*.1,
                        1,//1.0-c*100.0,

                        1-t/MAX_DIST*2
                        //sin(t*10.0-iTime*.1)

                        
                        //EPS/(abs(d))
                
                        
                ));
    //return fract(ro+rd*t)/d*.02;
    
    //return rd;
}

float getProceduralColors(inout vec3 diffuse, inout vec3 specular, inout float shininess) {
    specular = vec3(0);
    shininess = 127;


    vec3 surface_pos = _position.xyz*iWorldScale;
    vec3 eye_pos = inverse(iWorldOrientation)*(getEyeWorldPos()-iWorldPosition);
//    surface_pos.y +=  iWorldScale.y/2;
//    eye_pos.y +=  iWorldScale.y/2;
    // eye/surface space:
    // WorldScale
    // ObjectOrientation
    // Origin is bottom center

    vec3 ray_dir = normalize(surface_pos - eye_pos);

    diffuse=scene(eye_pos, ray_dir);

    /*



    	float tim=iTime;
	vec2 uv=fragCoord.xy/iResolution.xy;
	tim*=0.5;
	
    vec3 ro=vec3(cos(tim),cos(tim*0.3)*0.5,cos(tim*0.7))*min(0.5+tim*0.1+cos(tim*0.4)*0.5,1.5);
	vec3 rd=lookat(-ro)*normalize(vec3((fragCoord.xy-0.5*iResolution.xy)/iResolution.y,1.0));

  
    

	vec3 color=scene(ro,rd,fragCoord.xy);
	color=clamp(color,0.0,min(tim,1.0));
	fragColor = vec4(color,1.0);
    */
    return 1;
}
