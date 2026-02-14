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
    float t1;
    float t2;
} chunk_params;

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

float get_noise_3d(vec3 pos) {
    float noise = fbm(pos.xz / noise_params.scale, int(noise_params.octaves), noise_params.lacunarity, noise_params.gain) * chunk_params.chunkHeight;
	return noise - pos.y;
}

// The code we want to execute in each invocation
void main() {
    uvec3 position = gl_GlobalInvocationID.xyz;
    if (uint(position.x) >= uint(chunk_params.chunkWidth) || uint(position.y) >= uint(chunk_params.chunkHeight) || uint(position.z) >= uint(chunk_params.chunkWidth)) return;
    
    uint index = int(position.z) * int(chunk_params.chunkHeight) * int(chunk_params.chunkWidth) + int(position.y) * int(chunk_params.chunkWidth) + int(position.x);
    float height = get_noise_3d(vec3(position.x, position.y, position.z));

    vertices.data[index] = vec4(position.x, position.y, position.z, height);
}