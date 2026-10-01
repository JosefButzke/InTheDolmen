#[compute]
#version 450

layout(local_size_x = 8, local_size_y = 8, local_size_z = 8) in;

// A binding to the buffer we create in our script
layout(set = 0, binding = 0, std430) writeonly buffer Vertices {
    vec4 data[];
} vertices;

layout(set = 0, binding = 1, std140) uniform NoiseParams {
    float octaves;
    float lacunarity;
    float gain;
    float scale;
} noise_params;

layout(set = 0, binding = 2, std140) uniform ChunkParams {
    float chunkWidth;
    float chunkHeight;
    float resolution;
    float t2;
} chunk_params;

layout(set = 0, binding = 3, std140) uniform ChunkOffset {
    float x;
    float y;
    float z;
    float t3;
} chunk_offset;

// --- Hash / value noise (fast, decent for fBm) ---

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

#include "planet_noise.glslinc"

// The code we want to execute in each invocation
void main() {
    uvec3 position = gl_GlobalInvocationID.xyz;
    if (uint(position.x) >= uint(chunk_params.chunkWidth/chunk_params.resolution) || uint(position.y) >= uint(chunk_params.chunkHeight/chunk_params.resolution) || uint(position.z) >= uint(chunk_params.chunkWidth/chunk_params.resolution)) return;
    
    uint index = int(position.z) * int(chunk_params.chunkHeight/chunk_params.resolution) * int(chunk_params.chunkWidth/chunk_params.resolution) + int(position.y) * int(chunk_params.chunkWidth/chunk_params.resolution) + int(position.x);
    float height = noiseSplitter(vec3(position.x * chunk_params.resolution + chunk_offset.x, position.y * chunk_params.resolution + chunk_offset.y, position.z * chunk_params.resolution + chunk_offset.z));

    vertices.data[index] = vec4(position.x * chunk_params.resolution, position.y * chunk_params.resolution, position.z * chunk_params.resolution, height);
}