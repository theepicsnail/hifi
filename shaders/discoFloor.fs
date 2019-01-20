/*

{
  "ProceduralEntity": {
    "shaderUrl": "http://home.snail.rocks/shaders/discoFloor.fs?1",
    "version": 2,
    "uniforms": {
    },
    "channels": []
  }
}
*/

uniform float tile_size = 0.3;
uniform float fps=1;

float FRAME = iGlobalTime * fps;
float FRAME_ID = floor(FRAME);
float FRAME_FRACT = fract(FRAME);
vec3 HUEtoRGB(float H) {
    H = fract(H);
    float R = abs(H * 6 - 3) - 1;
    float G = 2 - abs(H * 6 - 2);
    float B = 2 - abs(H * 6 - 4);
    return vec3(R,G,B);
}

mat2x2 rot(float r) {
    float c = cos(r);
    float s = sin(r);
    return mat2x2(c,-s,s,c);
}

vec3 chaos_ripples(vec2 id, vec2 uv) {
    float r = (length(id))*0.1;
    r *= FRAME * .01 + 100;
    r -=  floor(FRAME) * .1;
    return HUEtoRGB(r);
}


vec3 spiral_sweep(vec2 id, vec2 uv) {
    vec2 center = vec2(sin(iGlobalTime)*5, cos(iGlobalTime)*5) * 0;
    vec2 pos = id - center;

    pos = rot(iGlobalTime) * pos;

    float r = (length(pos)) * .1;
    r -=  floor(FRAME) * .01;
    
    r += atan(pos.y, pos.x) * sin(iGlobalTime/2) + iGlobalTime;
    return HUEtoRGB(r);
}
float rand(vec2 id){ return fract(sin(dot(id, vec2(12.34, 78.61))*15.16)*13.01); }
vec3 random_colors(vec2 id, vec2 uv) {
    return HUEtoRGB(iGlobalTime * rand(id));
}

vec3 fbm_colors(vec2 id, vec2 uv) {
    float i = floor(iGlobalTime*.25);
    float f = fract(iGlobalTime*.25);
    float a = mix(
        hifi_fbm(id*.05 + i),
        hifi_fbm(id*.05 + i + 1),
        f
    );
    return HUEtoRGB(iGlobalTime*.5 + a*2);
}

vec3 h_colors(vec2 id, vec2 uv) {
    float offset = sin(iGlobalTime+4);
    return HUEtoRGB(id.x*.1 + offset);
}
vec3 v_colors(vec2 id, vec2 uv) {
    float offset = cos(iGlobalTime+2);
    return HUEtoRGB(id.y*.1 + offset);
}

vec3 h_colors2(vec2 id, vec2 uv) {
    float offset = iGlobalTime * (mod(floor(id.y/6),2)*2-1);
    return HUEtoRGB(id.x*.1 + offset);
}


vec3 spotlights(vec2 id, vec2 uv) {
    vec3 rad = vec3(10,5,7);
    vec3 angles = vec3(.5, 1.1, .3) * iGlobalTime;
    float r = length(id - rot(angles.r) * vec2(rad.r,0));
    float g = length(id - rot(angles.g) * vec2(rad.g,0));
    float b = length(id - rot(angles.b) * vec2(rad.b,0));
    return clamp(cos(vec3(r,g,b)-angles)*.5+.5, 0, 1);
}

vec3 checkers(vec2 id, vec2 uv) {
    float n = abs(mod(id.x,5)-2)*.2;// //abs(mod(id.x,3.0)-1.0) + abs(mid(id.y,3)-1);
    n += abs(mod(id.y,5)-2)*.2;

    float h = n * .5+ iGlobalTime*.2;
    return HUEtoRGB(h);
}





void effect1(inout vec2  id,inout vec2  uv) {
    //id += .125;//(uv*2-1)*sin(iGlobalTime*10+3);
    float scale = 1 + sin(iGlobalTime*.7)*.5;
    id = floor(rot(iGlobalTime*.6) * id * scale);
}
void effect2(inout vec2  id,inout vec2  uv) {
    id = rot(.1*length(id)*sin(iGlobalTime*.3))*id;
}
void effect3(inout vec2  id,inout vec2  uv) {
    id *= (sin(iGlobalTime*.9348+.3)*.5+1);
}
void effect4(inout vec2  id,inout vec2  uv) {
    uv = floor(uv*10)/10;
    id +=min(uv,1-uv);
}

vec3 getColor(float i, vec2 id, vec2 uv) {
    int c = int(fract(i) * 6);
    int e = int(fract(i) * 5);
    //e=0;
    switch(e) {
        case 0: break;
        case 1: effect1(id, uv); break;
        case 2: effect2(id, uv); break;
        case 3: effect3(id, uv); break;
        case 4: effect4(id, uv); break;
    }
    //return checkers(id, uv);
    switch(c) {
        case 0: return h_colors(id, uv);
        case 1: return h_colors2(id, uv);
        case 2: return v_colors(id, uv);
        case 3: return spotlights(id, uv);
        case 4: return checkers(id, uv);
        case 5: return fbm_colors(id, uv);
        case 6: return chaos_ripples(id, uv);
    }
    return vec3(1,0,1);
}

vec3 all_colorings(vec2 id, vec2 uv) {
    float phase = iGlobalTime*.5+20;
    float i = floor(phase);
    float f = fract(phase);
    float r1 = rand(vec2(i,0));
    float r2 = rand(vec2(i+1,0));

    return mix(
        getColor(r1, id, uv),
        getColor(r2, id, uv),
        f);
}


vec3 color_tile(vec2 id, vec2 uv) {
    

    //float transition = sin(iGlobalTime*1);

    //color = mix(color, color2, clamp(transition, -.5, .5)+.5);
    vec3 color = all_colorings(id, uv);
    float F = floor(iGlobalTime*2);
    float P = fract(iGlobalTime*2);
    //color.r*= mix(rand(id*F), rand(id*(F+1)), P);
    //color.g*= mix(rand(id*(F+1)), rand(id*(F+2)), P);
    //color.b*= mix(rand(id*(F+2)), rand(id*(F+3)), P);
    vec4 tile_mask = texture(iChannel0, uv);
    return color * tile_mask.rgb;
}


float getProceduralColors(inout vec3 diffuse, inout vec3 specular, inout float shininess) {
    
    specular = vec3(0,0,0);
    diffuse = vec3(0,0,0);
    shininess = 1;

    vec3 surface_pos = _position.xyz*iWorldScale;

    vec2 tile_uv = surface_pos.xz/tile_size;
    vec2 tile_space_uv = fract(tile_uv);
    vec2 tile_space_id = floor(tile_uv);

    
    diffuse.rgb = color_tile(tile_space_id, tile_space_uv);
    
    return 1;
}
