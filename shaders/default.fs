/*
Recommended:
    grabbble: off
    collides: off

Example User Data:

{
  "ProceduralEntity": {
    "shaderUrl": "http://home.snail.rocks:8000/shaders/default.fs",
    "version": 2,
    "channels": [
        "https://www.outworldz.com/SeamlessTextures/master/UV%20Checker/uv_checker%20large.png"
    ]
  }
}
*/

float getProceduralColors(inout vec3 diffuse, inout vec3 specular, inout float shininess) {
    shininess = 127;
    return 0;
}
