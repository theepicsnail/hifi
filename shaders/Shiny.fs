/*
Recommended:
    grabbble: off
    collides: off

Example User Data:
{
  "ProceduralEntity": {
    "shaderUrl": "https://theepicsnail.github.io/hifi/shaders/Shiny.fs",
    "version": 2,
    "channels": [],
    "grabbableKey": {
      "grabbable": false
    }
  }
}
*/

const float PI = 3.14159265359;
float getProceduralColors(inout vec3 diffuse, inout vec3 specular, inout float shininess) {
    vec3 worldEye = getEyeWorldPos();
    
    
    vec3 ro = _position.xyz;
    vec3 eye = (
        //inverse(iWorldOrientation)*
        (worldEye - iWorldPosition)*inverse(iWorldOrientation))
        / iWorldScale;

    vec3 rd = normalize(ro - eye);
    

    //diffuse = fract(float(iFrameCount)/1000.0).xxx;
    diffuse = fract(ro*inverse(iWorldOrientation)*7);
    diffuse -= diffuse.yzx;
    diffuse -= diffuse.zxy;
    diffuse = abs(diffuse);
    specular = 1-diffuse;
    shininess = 0.5;
    return 1.0;
}
