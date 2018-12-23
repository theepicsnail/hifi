/**

{
  "ProceduralEntity": {
    "shaderUrl": "http://home.snail.rocks:8000/shaders/ledPanel.fs",
    "version": 2,
    "channels": [
        "http://home.snail.rocks:8000/shaders/snailBillboard.png",
        "http://home.snail.rocks:8000/shaders/pixelMask.png"
    ],
    "uniforms": {
        "scale": [1600, 400],
        "brightness": 3
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


uniform float PIXEL_SIZE = .002; // meters
float getProceduralColors(inout vec3 diffuse, inout vec3 specular, inout float shininess) {
    shininess = 0;
    specular = vec3(0);
    diffuse = vec3(0);

    vec3 worldEye = getEyeWorldPos();
    vec3 worldScalePosition = _position.xyz * iWorldScale;
    vec3 eye = (inverse(iWorldOrientation) * (worldEye - iWorldPosition));
    float dist = length(worldScalePosition-eye);
    

    vec2 panel_uv = (.5-_position.xy)*iWorldScale.xy; // 0-1
    //vec2 scaledPos = vec2(rows,cols);
    vec2 pixel_space = panel_uv.xy/PIXEL_SIZE;
    vec2 sample_space = floor(pixel_space)*PIXEL_SIZE/iWorldScale.xy;

    diffuse = texture(iChannel0, sample_space).xyz
            * mix(
                texture(iChannel1, pixel_space).xyz,
                vec3(1),
                clamp(
                    atan(dist / PIXEL_SIZE / 500 )-.2
                , 0, 1)*0.9
            );
    
    //diffuse.xy = fract(abs(_position.xy*3));
    return 0;
}
