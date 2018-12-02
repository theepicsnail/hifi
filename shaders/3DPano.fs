/*
Snail's 3D over under pano viewer.

Recommended:
    grabbble: off
    collides: off
Example User Data:
{
  "ProceduralEntity": {
    "shaderUrl": "https://theepicsnail.github.io/hifi/shaders/3DPano.fs",
    "version": 2,
    "channels": [
        "http://blog.dsky.co/wp-content/uploads/2015/09/06-VikingVillage_stereo_thumb.jpg"
    ],
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
    vec3 eye = (inverse(iWorldOrientation) * (worldEye - iWorldPosition)) / iWorldScale;

    vec3 rd = normalize(ro - eye);

    float theta = atan(rd.z, rd.x);
    float phi = asin(rd.y);

    vec2 uv = vec2(
        theta/PI*.5+.5,
        phi/PI*.5 +.5
    );

    uv.y = rd.y*-.5+.5;
    uv.y = uv.y*.5  + cam_getStereoSide()*.5;
    diffuse = texture(iChannel0, uv).xyz;
    specular = diffuse;
    shininess = 0.5;
    return 1.0;
}
