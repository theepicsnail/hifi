/*
Snail's 3D dice.
{
  "ProceduralEntity": {
    "shaderUrl": "https://theepicsnail.github.io/hifi/shaders/dice.fs",
    "version": 2,
    "channels": [],
    "grabbableKey": {
      "grabbable": false
    }
  }
}
rougeau
*/

void raySphereIntersect(vec3 r0, vec3 rd, vec3 s0, float sr, inout float best, inout vec3 normal) {
    // - r0: ray origin
    // - rd: normalized ray direction
    // - s0: sphere center
    // - sr: sphere radius
    //
    // - best: current closest distance to the camera, updated if this is closer.
    // - normal: normal position on the sphere, updated if we're closer than best.
    float a = dot(rd, rd);
    vec3 s0_r0 = r0 - s0;
    float b = 2.0 * dot(rd, s0_r0);
    float c = dot(s0_r0, s0_r0) - (sr * sr);
    if (b*b - 4.0*a*c < 0.0) {
        return;
    }
    float d = (-b - sqrt((b*b) - 4.0*a*c))/(2.0*a);
    if(d > 0 && d < best) {
        best = d;
        normal = normalize(r0+rd*d - s0);
    }
}

float getProceduralColors(inout vec3 diffuse, inout vec3 specular, inout float shininess) {
    
    specular = vec3(0,0,0);
    diffuse = vec3(1,1,1);
    shininess = 1;
    // _position is in [-0.5, 0.5]
    float threshold = .45; 
    int xp = _position.x > threshold ? 1 : 0;
    int yp = (_position.y) > threshold ? 1 : 0;
    int zp = (_position.z) > threshold ? 1 : 0;
    int xn = (-_position.x) > threshold ? 1 : 0;
    int yn = (-_position.y) > threshold ? 1 : 0;
    int zn = (-_position.z) > threshold ? 1 : 0;

    if(xp+yp+zp+xn+yn+zn > 1) {
        return 0;
    }
 
    int face = int(dot(vec3(xp,yp,zp), vec3(1,2,3)) + 
                   dot(vec3(xn,yn,zn), vec3(6,5,4)));
    // color table:
    //   r g b
    // 1 1 0 0 xp
    // 2 0 1 0 yp
    // 3 0 0 1 zp
    // 4 1 1 0 zn
    // 5 1 0 1 yn
    // 6 0 1 1 xn
    diffuse = vec3(
    //  1  2  3  4  5  6
        xp+      zn+yn,
           yp+   zn+   xn,
              zp+   yn+xn
    );
    
    //vec3 worldPos = iWorldOrientation*(_position.xyz*iWorldScale)+iWorldPosition;
    //vec3 eye_pos = getEyeWorldPos();
    // mul(rotation, obj_space_pos * scale) + offset = world space
    // eye_pos = world space.
    // Do some transforms to make the math nicer,
    // we want ceneterd in the dice, axis aligned, with world-space lengths.
    //
    // mul(rotation, obj_space_pos * scale) + offset = worldSpace = eye_pos
    // mul(rotation, obj_space_pos * scale) + offset = eye_pos
    // mul(rotation, obj_space_pos * scale) = eye_pos - offset
    // mul(inv(rotation), mul(rotation, obj_space_pos * scale)) = mul(inv(rotation), eye_pos - offset)
    // obj_space_pos * scale = mul(inv(rotation), eye_pos - offset)
    //
    // We now have all the rotation/offset pushed into the camera
    // and the object space position is scaled to world scale. 

    vec3 surface_pos = _position.xyz*iWorldScale;
    vec3 eye_pos = inverse(iWorldOrientation)*(getEyeWorldPos()-iWorldPosition);
    
    // Scale the sphere's radius by the smallest dimension.
    float rScale = min(iWorldScale.x, min(iWorldScale.y, iWorldScale.z));
    rScale *= .125; // keeps diapeter <= 1/4th of the box dimensions.

    vec3 posScale = iWorldScale*.25; // position pips +-.5 along each axis.

    // Actual ray tracing. Replaced SDF based with ray-sphere intersection calulations,
    // this is faster and yields better results for this use case.    
    vec3 ray_dir =normalize(surface_pos-eye_pos);
    vec3 ray_origin = surface_pos;
    //if(sign(ray_origin) == sign(ray_dir))
    //return 0;
    
    float MAX_DISTANCE = 1000;// hopefully we never need a dice >1km cubed...
    float best = MAX_DISTANCE; 
    vec3 norm = vec3(0,0,0);
    switch(face){
        case 6:
        // side pips
        raySphereIntersect(ray_origin, ray_dir, posScale * vec3(0,1,0), rScale, best, norm);
        raySphereIntersect(ray_origin, ray_dir, -posScale * vec3(0,1,0), rScale, best, norm);
        case 5: 
        case 4:
        // alternative corner pips
        raySphereIntersect(ray_origin, ray_dir, posScale*vec3( 1, 1, 1), rScale, best, norm);
        raySphereIntersect(ray_origin, ray_dir, posScale*vec3( 1,-1,-1), rScale, best, norm);
        raySphereIntersect(ray_origin, ray_dir, posScale*vec3(-1, 1,-1), rScale, best, norm);
        raySphereIntersect(ray_origin, ray_dir, posScale*vec3(-1,-1, 1), rScale, best, norm);
        break;

        case 3:
        case 2:
        // corner pips
        raySphereIntersect(ray_origin, ray_dir, posScale, rScale, best, norm);
        raySphereIntersect(ray_origin, ray_dir, -posScale, rScale, best, norm);
    }
    if(face==1||face==3||face==5) {
        // center
        raySphereIntersect(ray_origin, ray_dir, vec3(0,0,0), rScale, best, norm);
    }
    
    // None of the sphere checks hit. Leave this as transparent. 
    if(best == MAX_DISTANCE) discard;
    
    // Do some ambient/normal lighitng.
    diffuse *= -dot(normalize(ray_dir), norm);
    
    return 0;
}
