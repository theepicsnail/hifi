/*
Recommended:
    grabbble: off
    collides: off

Example User Data:

{
  "ProceduralEntity": {
    "shaderUrl": "http://home.snail.rocks:8000/shaders/building.fs",
    "version": 2,
    "channels": [
        "https://www.outworldz.com/SeamlessTextures/master/UV%20Checker/uv_checker%20large.png"
    ]
  }
}
*/
const float PI = 3.1415926;
const float TAU = 2*PI;
const vec2 SCALE = vec2(.5,.5);

vec3 getInterior() {
    vec3 surface_pos = _position.xyz*iWorldScale;
    vec3 eye_pos = inverse(iWorldOrientation)*(getEyeWorldPos()-iWorldPosition);
    vec3 rd = normalize(surface_pos - eye_pos);
    surface_pos = (_position.xyz+.5)*iWorldScale * SCALE.xyx;
    vec3 room_id = floor(surface_pos+rd*.01);
    vec3 ro = fract(surface_pos+rd*.01);

    vec3 signs = max(vec3(0),sign(rd)); // 0 to 1
    vec3 destWall = signs + floor(ro);
    vec3 distToWall = (destWall-ro)/rd;


    vec3 p = vec3(0,0,0);
    if(distToWall.x < distToWall.y && distToWall.x < distToWall.z) {
        vec2 wall = (ro + rd * distToWall.x).yz;
        p = vec3(signs.x,wall.x,wall.y);
    }
    if (distToWall.y < distToWall.x && distToWall.y < distToWall.z) {
        vec2 wall = (ro + rd * distToWall.y).xz;
        p = vec3(wall.x,signs.y,wall.y);
    }
    if (distToWall.z < distToWall.x && distToWall.z < distToWall.y) {
        vec2 wall = (ro + rd * distToWall.z).xy;
        p = vec3(wall.x,wall.y,signs.z);
    }
    p-=.5;
    vec3 n = normalize(p);

    //lol
    float c = cos(iGlobalTime);
    float s = sin(iGlobalTime);
    n.xz =mat2x2(c,-s,s,c) * n.xz;

    //return n;
    vec2 uv = vec2(
        atan(n.z, n.x)/TAU,
        1-asin(n.y)/PI
    ) + .5;

    float f = fract(sin(dot(room_id, vec3(135.134343, 981.3444, 12.403)))*1353.123);
    
    if(.5 < fract(iGlobalTime*.3+.5+f))
      return texture(iChannel1, uv).xyz;
      return texture(iChannel2, uv).xyz;
    //discard;
}

float getProceduralColors(inout vec3 diffuse, inout vec3 specular, inout float shininess) {
    shininess = 0*128;
    specular = vec3(0);
    diffuse = vec3(1);

    vec3 surface_pos = (_position.xyz+.5)*iWorldScale * SCALE.xyx;
    
    vec3 local_normal = normalize((_normal.xyz * iWorldOrientation).xyz);

    vec2 uv = vec2(
        surface_pos.x * local_normal.z +
        surface_pos.z * local_normal.x,
        1-surface_pos.y
    );
    uv = uv+.01;
    // lol
    uv.x += iGlobalTime*0.5*(mod(floor(uv.y),2.0)*2.0-1.0);

    vec4 wall_color = texture(iChannel0, uv);
    //vec2(dot(surface_pos.xz, normal.zx),surface_pos.y);
    diffuse = wall_color.xyz;
    if(wall_color.w < .9) {
        diffuse = getInterior();
        shininess = 128;
    }
    return 0;
}
