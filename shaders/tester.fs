/*
Recommended:
    grabbble: off
    collides: off

Example User Data:

{
  "ProceduralEntity": {
    "shaderUrl": "http://home.snail.rocks:8000/shaders/tester.fs",
    "version": 2,
    "uniforms": {
        "_shininess":0,
        "_diffuse":[0,0,0],
        "_specular":[0,0,0],
        "_emissive":0
    },
    "channels": []
  }
}

*/

uniform float _shininess;
uniform vec3 _specular;
uniform vec3 _diffuse;
uniform float _emissive;

float getProceduralColors(inout vec3 odiffuse, inout vec3 ospecular, inout float oshininess) {
    oshininess = _shininess;
    ospecular = _specular;
    odiffuse = _diffuse;
    return _emissive;
}
