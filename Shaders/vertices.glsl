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

// 3D Simplex Noise

vec4 permute(vec4 x) {
    return mod(((x * 34.0) + 1.0) * x, 289.0);
}

vec4 taylorInvSqrt(vec4 r) {
    return 1.79284291400159 - 0.85373472095314 * r;
}

float snoise(vec3 v) {
    const vec2  C = vec2(1.0/6.0, 1.0/3.0);
    const vec4  D = vec4(0.0, 0.5, 1.0, 2.0);

    // First corner
    vec3 i  = floor(v + dot(v, C.yyy));
    vec3 x0 = v - i + dot(i, C.xxx);

    // Other corners
    vec3 g = step(x0.yzx, x0.xyz);
    vec3 l = 1.0 - g;
    vec3 i1 = min(g.xyz, l.zxy);
    vec3 i2 = max(g.xyz, l.zxy);

    vec3 x1 = x0 - i1 + C.xxx;
    vec3 x2 = x0 - i2 + C.yyy;
    vec3 x3 = x0 - D.yyy;

    // Permutations
    i = mod(i, 289.0);
    vec4 p = permute(
                permute(
                    permute(i.z + vec4(0.0, i1.z, i2.z, 1.0))
                  + i.y + vec4(0.0, i1.y, i2.y, 1.0))
              + i.x + vec4(0.0, i1.x, i2.x, 1.0));

    // Gradients
    float n_ = 1.0 / 7.0; // 7x7 points over a square
    vec3 ns = n_ * D.wyz - D.xzx;

    vec4 j = p - 49.0 * floor(p * ns.z * ns.z);

    vec4 x_ = floor(j * ns.z);
    vec4 y_ = floor(j - 7.0 * x_);

    vec4 x = x_ * ns.x + ns.y;
    vec4 y = y_ * ns.x + ns.y;
    vec4 h = 1.0 - abs(x) - abs(y);

    vec4 b0 = vec4(x.xy, y.xy);
    vec4 b1 = vec4(x.zw, y.zw);

    vec4 s0 = floor(b0) * 2.0 + 1.0;
    vec4 s1 = floor(b1) * 2.0 + 1.0;
    vec4 sh = -step(h, vec4(0.0));

    vec4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
    vec4 a1 = b1.xzyw + s1.xzyw * sh.zzww;

    vec3 p0 = vec3(a0.xy, h.x);
    vec3 p1 = vec3(a0.zw, h.y);
    vec3 p2 = vec3(a1.xy, h.z);
    vec3 p3 = vec3(a1.zw, h.w);

    // Normalize gradients
    vec4 norm = taylorInvSqrt(vec4(dot(p0,p0), dot(p1,p1), dot(p2,p2), dot(p3,p3)));
    p0 *= norm.x;
    p1 *= norm.y;
    p2 *= norm.z;
    p3 *= norm.w;

    // Mix contributions
    vec4 m = max(0.6 - vec4(dot(x0,x0), dot(x1,x1), dot(x2,x2), dot(x3,x3)), 0.0);
    m = m * m;

    return 42.0 * dot(m*m, vec4(
        dot(p0,x0),
        dot(p1,x1),
        dot(p2,x2),
        dot(p3,x3)
    ));
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

float fbm3d(int numLayers, float lacunarity, float persistence, float scale, vec3 pos) {
    float sum = 0.0;
    float frequency = scale / 100.0;
    float amplitude = 1.0;

    for (int i = 0; i < numLayers; ++i) {
        float s = snoise(pos * frequency);     // ~[-1, 1]
        float n = 1.0 - abs(s * 2.0 - 1.0);    // same shaping as your HLSL
        // n *= n;                              // optional: sharper ridges
        sum += n * amplitude;

        amplitude *= persistence;
        frequency *= lacunarity;
    }
    return sum;
}

// Smooth step helper for "soft" cave walls in density fields.
float smoothThreshold(float x, float threshold, float softness) {
    // returns ~0 below threshold, ~1 above threshold, with smooth transition
    return smoothstep(threshold - softness, threshold + softness, x);
}

// Height preference: 1.0 at cave band center, falls off above/below.
float heightBand(float y, float centerY, float halfWidth) {
    float d = abs(y - centerY) / max(halfWidth, 1e-5);
    return clamp(1.0 - d, 0.0, 1.0);
}

float noiseSplitter(vec3 p) {
    // air 0 or lesser / ground 0 or bigger
    float noise = -1.0;
    float cavesFloorLevel = -(chunk_params.chunkHeight/2);
    float baseFloorLevel = 0.0;
    float peaksLevel = chunk_params.chunkHeight - 1.0;

    // underground floor
    if(p.y == cavesFloorLevel) {
        return 1.0;
    }

    // SURFACE
    if(p.y > baseFloorLevel) {
        float noise = fbm(p.xz / noise_params.scale, int(noise_params.octaves), noise_params.lacunarity, noise_params.gain) * chunk_params.chunkHeight;
        return noise - p.y;
    }

    // CAVES
    // float fbm3d(int numLayers, float lacunarity, float persistence, float scale, vec3 pos) {
    noise = fbm3d(3, 0.4, 3.0, 16, p);
    return noise;
}

// The code we want to execute in each invocation
void main() {
    uvec3 position = gl_GlobalInvocationID.xyz;
    if (uint(position.x) >= uint(chunk_params.chunkWidth) || uint(position.y) >= uint(chunk_params.chunkHeight) || uint(position.z) >= uint(chunk_params.chunkWidth)) return;
    
    uint index = int(position.z) * int(chunk_params.chunkHeight) * int(chunk_params.chunkWidth) + int(position.y) * int(chunk_params.chunkWidth) + int(position.x);
    float height = noiseSplitter(vec3(position.x + chunk_offset.x, position.y + chunk_offset.y, position.z + chunk_offset.z));

    vertices.data[index] = vec4(position.x, position.y, position.z, height);
}