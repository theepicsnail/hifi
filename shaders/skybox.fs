/*
{
  "ProceduralEntity": {
    "shaderUrl": "https://shaderclass.glitch.me/basic.fs?150",
    "version": 2,
    "channels": [
      "https://previews.123rf.com/images/aelita2/aelita21501/aelita2150100024/35328543-mask-overlay-grunge-texture-painted-background-.jpg"
    ],
    "uniforms": {
      "flashing": 1,
      "frequency": 1,
      "color": [
        1,
        1,
        0
      ]
    }   
  }
}
*/

uniform float flashing;
uniform float frequency;
uniform vec3 color;

/*
float getProceduralColors(inout vec3 diffuse, inout vec3 specular, inout float shininess) {
    specular = vec3(0,0,0);
    diffuse = color * (
      texture(iChannel0, _position.xy).xyz +
      texture(iChannel0, _position.yz).xyz +
      texture(iChannel0, _position.zx).xyz );
    shininess = 0;
    return sin(iGlobalTime*frequency) * flashing;
}*/

const float PI  = 3.14159;
const float TAU = 6.28318;
float rand(float n){return fract(sin(n) * 43758.5453123);}

float interpolate(float a, float b, float c, float d, float t) {
// data: a--b--c--d
// time:    0--1
    float m1 = (c-a)/2;
    float m2 = (d-b)/2;
    float t2 = t*t;
    float t3 = t2*t;
    return dot(vec4(b,m1,c,m2), vec4(2*t3-3*t2+1, t3-2*t2+t, 3*t2-2*t3, t3-t2));
}
vec3 getSkyboxColor()
{
    vec3 n = normalize(_normal);
    float around = atan(n.x,n.z); // -pi to pi
    float down = acos(n.y); // 0 to pi
    around = around/TAU+.5; // 0 to 1
    down /= PI; // 0 to 1
    
    float t = iGlobalTime * .1;
    float i = floor(t);
    float v = interpolate(rand(i-1),rand(i),rand(i+1),rand(i+2), fract(t));

    float d = (down - v)*100;
    float b = cos(d)/abs(d);
    d = cos((around-v)*PI*4)*PI;
    b += cos(d)/abs(d);
    b/= 2;

    return vec3(1,1,1) * b;
}