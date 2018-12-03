/*
Recommended:
    grabbble: off
    collides: off

Example User Data:
{
  "ProceduralEntity": {
    "shaderUrl": "https://theepicsnail.github.io/hifi/shaders/Spheres.fs",
    "version": 2,
    "channels": [],
    "grabbableKey": {
      "grabbable": false
    }
  }
}
*/

float noise(vec3 p) {
    return fract(
        sin(
            dot(
                p,
                vec3(12.9898, 78.233, 52.24545)
            )
        )*43758.5453123);
}


const float PI = 3.14159265359;
const float TAU = PI * 2;
float getProceduralColors(inout vec3 diffuse, inout vec3 specular, inout float shininess) {
    specular = vec3(0);
    shininess = 0.5;
    
    vec3 worldEye = getEyeWorldPos();

    vec3 localPos = _position.xyz;

    vec3 worldPos =  (iWorldOrientation * (localPos*iWorldScale)) + iWorldPosition;
    vec3 viewDirection = normalize(worldPos-worldEye);
 
    vec2 uv = worldPos.xz + iGlobalTime*.1;
    float speed = .1;
    vec2 d1 = vec2(cos(0),sin(0))* speed;
    vec2 d2 = vec2(cos(2),sin(2))* speed;
    vec2 d3 = vec2(cos(4),sin(4))* speed;
    vec3 c1 =(texture(iChannel0, worldPos.xz  + iGlobalTime * d1).xyz +
              texture(iChannel0, worldPos.zx*vec2(1.1,.9)  + iGlobalTime * d2).xyz +
              texture(iChannel0, worldPos.xz*vec2(.9,1.1)  + iGlobalTime * d3).xyz
    )/3;

    vec3 c2 = texture(iChannel1, worldPos.xz).xyz;
    vec3 c3 = texture(iChannel2, worldPos.xz).xyz;
    //c3 += texture(iChannel2, worldPos.xz*vec2(.86, .5) +  worldPos.zx*vec2(-.5, .86)).xyz;
    //c3 /= 2;

    float alt = hifi_fbm(worldPos.xz*.1);
    
    
    // 0 - water
    float b1 = mix(.18, .2, snoise(vec3(worldPos.xz*2, iGlobalTime * .1))); // solid water below here
    float b2 = .25; // solid water to solid dirt transition endpoint
    float b3 = .25; // solid dirt below here
    float b4 = .3; //to solid grass transition endpoint

    if(alt < b1) {
        diffuse = c1;
    } else if(alt < b2) { 
        diffuse = mix(c1, c2, (alt-b1)/(b2-b1));
    } else if(alt < b3) {
        diffuse = c2;
    } else if(alt < b4) { 
        diffuse = mix(c2, c3, (alt-b3)/(b4-b3));
    } else {
        diffuse = c3;
    }

    //diffuse = fract(alt.xxx);
    //fract(vec3(worldPos.xz,0));
    
    return 1;
}
