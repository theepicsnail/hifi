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
 
    vec3 pos = worldEye; 

    float dist = 0;
    int i = 0;
    int steps = 50;
    float d;
    vec3 p;
    for(i = 0 ; i < steps ; i ++) {
        p = pos + dist * viewDirection - iWorldPosition;
        vec3 cell_id = floor(p); // integer ids for each voxel
        vec3 cell_pos = fract(p) - .5; // -.5 to .5 per voxel

        float r = sin(
            snoise(vec3(cell_id.y*.05 - iGlobalTime*.2,
            cell_id.xz
            ))
            );// ;//mix(0, .25, fract(cell_id.y * .1));
        r *= r * r;
        d = length(cell_pos) - (r*.5 + .5)*.25;
        if(d < .001) {
            break;
        }
        dist += d;
    }
    if(i == steps) discard;

    // https://iquilezles.org/www/articles/palettes/palettes.htm
    vec3 ca = vec3(.5);
    vec3 cb = (.5 - abs(.5-ca));
    vec3 cc = vec3(
        snoise(vec4(p, 3)),
        snoise(vec4(p, 4)),
        snoise(vec4(p, 5))
    );
    vec3 cd = vec3(0, 2, 4) + snoise(vec4(p, 0))*PI;
        //snoise(vec4(p, 0)),
        //snoise(vec4(p, 1)),
        //snoise(vec4(p, 2))
    //);
    float noise_scale = 10;
    float n = noise(floor(p));
    if(n < .5) {
        noise_scale = .1;
    }
    float t = abs(snoise(p*noise_scale + iGlobalTime*.1))*.99;
    
    vec3 color = ca + cb * cos(TAU * cc * t+ cd);
    
    float brightness = -dot(viewDirection, normalize(fract(p)-.5));
    brightness /= max(dist,1);
    diffuse = brightness * color;
    
    return 1;
}
