/*
Recommended:
    grabbble: off
    collides: off

Example User Data:
{
  "ProceduralEntity": {
    "shaderUrl": "https://theepicsnail.github.io/hifi/shaders/LavaLamp.fs",
    "version": 2,
    "channels": [],
    "grabbableKey": {
      "grabbable": true
    }
  }
}
*/

float smin( float a, float b, float k )
{
    float h = max( k-abs(a-b), 0.0 );
    return min( a, b ) - h*h*0.25/k;
}

const float PI = 3.14159;
const float TAU= PI * 2;

float getProceduralColors(inout vec3 diffuse, inout vec3 specular, inout float shininess) {
    specular = vec3(0);
    shininess = 0.5;
    

    vec3 worldEye = getEyeWorldPos();

    vec3 localPos = _position.xyz;

    diffuse = fract(localPos);
    if(localPos.y < -.25 || localPos.y > .4) {
        diffuse = vec3(.01);
        return 1;
    }
    
    vec3 worldPos =  (iWorldOrientation * (localPos*iWorldScale)) + iWorldPosition;
    //vec3 rd = normalize(worldPos-worldEye);
    //vec3 ro = worldEye-iWorldPosition;
	
    vec3 ro = _position.xyz * iWorldScale;
    vec3 eye = (inverse(iWorldOrientation) * (worldEye - iWorldPosition)) ;
    vec3 rd = normalize((ro - eye));	

    float t = iGlobalTime + 1000;
    float dist = 0;
    int i = 0;
    int steps = 50;
    float d;
    vec3 p;
    for(i = 0 ; i < steps ; i ++) {
        p = ro + dist * rd;
        d = 100;
        for(int blob = 0 ; blob < 5 ; blob ++ ) {
            float blobR = mix(.04, .05, snoise(vec3(blob,0,t*.1)));
            float blobAmp = .5 - .1 * blob;
            float blobPhase = PI * snoise(vec3(blob,1,t*.02));
            float d2 = length(
                p
                -vec3(cos(blob), 0, sin(blob))*.02
                -vec3(0,1,0)
                *sin(t*.1 + blobPhase)
                *blobAmp
            ) - blobR;
            float m = smin(d,d2, 0.05);//, 0.1);//sminCubic(d, d2, .1);

            d = m;
        }
        
        if(d < .001) {
            break;
        }
        dist += d;
    }
    if(i == steps) {
        diffuse = vec3(.3,.4,1);
        return .01;

    } else {

        float percent = sin(dist*100)*.5+.5;
        diffuse = mix(vec3(1,.5,0), vec3(1,1,0), percent);
    }
    //fract(i*.1/steps).xxx*10;//vec3(dFdx(d), dFdy(d), 0)*1000;
    //diffuse =fract(p*10.0);

    
    return 1;

}
