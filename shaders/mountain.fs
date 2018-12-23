/*
Recommended:
    grabbble: off
    collides: off

Example User Data:

{
  "ProceduralEntity": {
    "shaderUrl": "http://home.snail.rocks:8000/shaders/mountain.fs",
    "version": 2,
    "channels": [
        "https://st2.depositphotos.com/3768069/5426/i/950/depositphotos_54266177-stock-photo-snow-seamless-texture-tile.jpg",
        "https://img1.cgtrader.com/items/853438/df987649b9/pbr-seamless-procedural-rock-textures-3d-model.jpg"
    ]
  }
}

*/

float getProceduralColors(inout vec3 diffuse, inout vec3 specular, inout float shininess) {
    shininess = 0;
    specular = vec3(0,0,0);
    
    float y = (_position.y+.5);
    y = pow(y, 3);
    
    diffuse = mix(texture(iChannel1, _position.xz).xyz,
                  texture(iChannel0, _position.xz).xyz,
                  y);
    specular.x =  1-max(diffuse.x, max(diffuse.y, diffuse.z)) ;
    shininess = specular.x* 127;
    //diffuse.xyz=fract(_position.yyy-.5);
    return 0;
}
