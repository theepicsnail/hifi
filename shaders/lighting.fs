
uniform float type;
float getProceduralColors(
    inout vec3 diffuse,
    inout vec3 specular,
    inout float shininess) {
    
    specular = vec3(0,0,0);
    shininess = 0;
    vec3 face = abs(_normalMS);

    vec3 pos = vec3(0.5) - _position.xyz;
    diffuse = mat3(
        texture(iChannel0, pos.zy).xyz,
        texture(iChannel0, pos.xz).xyz,
        texture(iChannel0, pos.xy).xyz) * face;
    return 1;
}

   