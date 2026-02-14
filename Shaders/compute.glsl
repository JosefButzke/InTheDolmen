#[compute]
#version 450

layout(local_size_x = 8, local_size_y = 8, local_size_z = 1) in;

layout(set = 0, binding = 0, rgba8) uniform writeonly image2D out_image;
layout(set = 0, binding = 1) uniform sampler2D in_tex;

// Parameter block (std140 alignment safe)
layout(set = 0, binding = 2, std140) uniform Params {
    float octaves;
    float lacunarity;
    float gain;
    float scale;
};


float hash12(vec2 x) {
    // Dave Hoskins style hash
    vec3 p3 = fract(vec3(x.xyx) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.x + p3.y) * p3.z);
}

float valueNoise(vec2 p2) {
    vec2 i = floor(p2);
    vec2 f = fract(p2);

    // Smoothstep-like fade
    vec2 u = f * f * (3.0 - 2.0 * f);

    float a = hash12(i + vec2(0.0, 0.0));
    float b = hash12(i + vec2(1.0, 0.0));
    float c = hash12(i + vec2(0.0, 1.0));
    float d = hash12(i + vec2(1.0, 1.0));

    float x1 = mix(a, b, u.x);
    float x2 = mix(c, d, u.x);
    return mix(x1, x2, u.y);
}

// --- fBm ---

float fbm(vec2 p2, int octaves, float lacunarity, float gain) {
    float sum = 0.0;
    float amp = 0.5;
    float freq = 1.0;

    for (int i = 0; i < octaves; i++) {
        sum += amp * valueNoise(p2 * freq);
        freq *= lacunarity;
        amp  *= gain;
    }
    return sum;
}


void main() {
    ivec2 pixel = ivec2(gl_GlobalInvocationID.xy);

    int oct = clamp(int(octaves), 1, 16);
    vec2 p = vec2(pixel) * scale;
    float noise_value = fbm(p/1000, oct, lacunarity, gain);
    float ns = noise_value;

    // Write full red, no green/blue, full alpha
    imageStore(out_image, pixel, vec4(1.0, 1.0, 1.0, ns));
}