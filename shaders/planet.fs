/*
Planet
{
  "ProceduralEntity": {
    "shaderUrl": "https://theepicsnail.github.io/hifi/shaders/dice.fs",
    "version": 2,
    "channels": [
        "http://3dmitchell.com/images/earthmap1k.jpg",
    ]
  }
}
*/

const float PI = 3.1415926;
const float TAU = 2*PI;
float getProceduralColors(inout vec3 diffuse, inout vec3 specular, inout float shininess) {   
    specular = vec3(0,0,0);
    diffuse = vec3(1,1,1);
    shininess = 1;

    vec3 n = _position.xyz;
    vec2 uv = vec2(
        -atan(n.z, n.x)/TAU,
        1-asin(n.y)/PI
    ) + .5;

    vec2 uv2 = vec2(
        -atan(n.z, n.x)/TAU + iGlobalTime*.01,
        1-asin(n.y)/PI
    ) + .5;


    diffuse = texture(iChannel0, uv).xyz + texture(iChannel1, uv2).xxx;
    return 0;
}