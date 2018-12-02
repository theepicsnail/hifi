/*
Recommended:
    grabbble: off
    collides: off

Example User Data:
{
  "ProceduralEntity": {
    "shaderUrl": "https://theepicsnail.github.io/hifi/shaders/SterioView.fs",
    "version": 2,
    "channels": [
        "https://cdn.discordapp.com/attachments/413247180887949322/518642617211420678/20181201231835_1_vr.jpg",
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

    uv= _position.xy;

    uv.x = (uv.x-0.5) /2;
    uv.x += cam_getStereoSide() *.5;

    uv.y *= -1;
    uv.y += .5;
    diffuse = texture(iChannel0, uv).xyz;
    specular = diffuse;
    shininess = 0.5;
    return 1.0;
}
