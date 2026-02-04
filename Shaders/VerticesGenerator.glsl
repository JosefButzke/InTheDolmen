shader_type compute;

// Godot 4: pick a local size that matches your Unity [numthreads(8,8,8)]
layout(local_size_x = 8, local_size_y = 8, local_size_z = 8) in;

// Storage buffer (equivalent of RWStructuredBuffer<float4>)
layout(set = 0, binding = 0, std430) buffer Vertices {
    vec4 vertices[];
};

// Uniforms (equivalent of your globals)
uniform int chunkWidth;
uniform int chunkHeight;
uniform vec3 offset;
uniform int precision;


// --------------------- Utility ---------------------

float DistanceFromCenter(vec2 a) {
    vec2 d = vec2(0.0) - a;
    return sqrt(d.x * d.x + d.y * d.y);
}

float Rand(vec2 co) {
    return fract(sin(dot(co, vec2(12.9898, 78.233))) * 43758.5453);
}

float Noise(vec2 p) {
    vec2 i = floor(p);
    vec2 f = fract(p);

    float a = Rand(i);
    float b = Rand(i + vec2(1.0, 0.0));
    float c = Rand(i + vec2(0.0, 1.0));
    float d = Rand(i + vec2(1.0, 1.0));

    vec2 u = f * f * (3.0 - 2.0 * f);

    return (mix(a, b, u.x) +
        (c - a) * u.y * (1.0 - u.x) +
        (d - b) * u.x * u.y);
}


// --------------------- Simplex noise (ported) ---------------------

vec3 mod289(vec3 x) { return x - floor(x / 289.0) * 289.0; }
vec4 mod289(vec4 x) { return x - floor(x / 289.0) * 289.0; }

vec4 permute(vec4 x) {
    return mod289((x * 34.0 + 1.0) * x);
}

vec4 taylorInvSqrt(vec4 r) {
    return 1.79284291400159 - r * 0.85373472095314;
}

float snoise(vec3 v) {
    const vec2 C = vec2(1.0 / 6.0, 1.0 / 3.0);

    // First corner
    vec3 i  = floor(v + dot(v, vec3(C.y)));
    vec3 x0 = v - i + dot(i, vec3(C.x));

    // Other corners
    vec3 g = step(x0.yzx, x0.xyz);
    vec3 l = vec3(1.0) - g;
    vec3 i1 = min(g.xyz, l.zxy);
    vec3 i2 = max(g.xyz, l.zxy);

    vec3 x1 = x0 - i1 + vec3(C.x);
    vec3 x2 = x0 - i2 + vec3(C.y);
    vec3 x3 = x0 - 0.5;

    // Permutations
    i = mod289(i);
    vec4 p =
        permute(
            permute(
                permute(i.z + vec4(0.0, i1.z, i2.z, 1.0))
                + i.y + vec4(0.0, i1.y, i2.y, 1.0)
            )
            + i.x + vec4(0.0, i1.x, i2.x, 1.0)
        );

    vec4 j = p - 49.0 * floor(p / 49.0);

    vec4 x_ = floor(j / 7.0);
    vec4 y_ = floor(j - 7.0 * x_);

    vec4 x = (x_ * 2.0 + 0.5) / 7.0 - 1.0;
    vec4 y = (y_ * 2.0 + 0.5) / 7.0 - 1.0;

    vec4 h = 1.0 - abs(x) - abs(y);

    vec4 b0 = vec4(x.xy, y.xy);
    vec4 b1 = vec4(x.zw, y.zw);

    vec4 s0 = floor(b0) * 2.0 + 1.0;
    vec4 s1 = floor(b1) * 2.0 + 1.0;
    vec4 sh = -step(h, vec4(0.0));

    vec4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
    vec4 a1 = b1.xzyw + s1.xzyw * sh.zzww;

    vec3 g0 = vec3(a0.xy, h.x);
    vec3 g1 = vec3(a0.zw, h.y);
    vec3 g2 = vec3(a1.xy, h.z);
    vec3 g3 = vec3(a1.zw, h.w);

    vec4 norm = taylorInvSqrt(vec4(dot(g0, g0), dot(g1, g1), dot(g2, g2), dot(g3, g3)));
    g0 *= norm.x;
    g1 *= norm.y;
    g2 *= norm.z;
    g3 *= norm.w;

    vec4 m = max(0.6 - vec4(dot(x0, x0), dot(x1, x1), dot(x2, x2), dot(x3, x3)), 0.0);
    m = m * m;
    m = m * m;

    vec4 px = vec4(dot(x0, g0), dot(x1, g1), dot(x2, g2), dot(x3, g3));
    return 42.0 * dot(m, px);
}


// --------------------- FBM variants ---------------------

float fbm(int numLayers, float lacunarity, float persistence, float scale, vec3 pos) {
    float nsum = 0.0;
    float frequency = scale / 100.0;
    float amplitude = 1.0;

    for (int i = 0; i < numLayers; i++) {
        float n = 1.0 - abs(snoise(pos * frequency) * 2.0 - 1.0);
        nsum += n * amplitude;

        amplitude *= persistence;
        frequency *= lacunarity;
    }
    return nsum;
}

float hash2(vec2 p) {
    return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453123);
}

float noise2(vec2 p) {
    vec2 i = floor(p);
    vec2 f = fract(p);

    float a = hash2(i);
    float b = hash2(i + vec2(1.0, 0.0));
    float c = hash2(i + vec2(0.0, 1.0));
    float d = hash2(i + vec2(1.0, 1.0));

    vec2 u = f * f * (3.0 - 2.0 * f);
    return mix(mix(a, b, u.x), mix(c, d, u.x), u.y);
}

float fbm2(vec2 p, float amplitude, float frequency) {
    float value = 0.0;
    for (int i = 0; i < 5; i++) {
        value += noise2(p * frequency) * amplitude;
        frequency *= 2.0;
        amplitude *= 0.5;
    }
    return value;
}


// --------------------- Terrain logic ---------------------

float noiseSplitter(float x, float y, float z) {
    // air < 0 / ground >= 0 (as per your comment)
    float noise = -1.0;
    float cavesFloorLevel = -(float(chunkHeight - precision));
    float baseFloorLevel = 0.0;
    // float peaksLevel = float(chunkHeight - precision); // kept but unused like your code

    if (DistanceFromCenter(vec2(x, z)) <= 96.0 && y >= 0.0) {
        if (y < 24.0) return 1.0;
        return -1.0;
    }

    // underground floor
    if (y == cavesFloorLevel) {
        return 1.0;
    }

    // SURFACE
    if (y > baseFloorLevel) {
        float scale = 256.0;
        noise = fbm2(vec2(x, z) / scale, 0.3, 1.4);
        return (noise * float(chunkHeight) - 8.0) - y;
    }

    // CAVES
    noise = fbm(3, 3.0, 0.2, 1.0, vec3(x, z, y));
    return noise;
}


// --------------------- Main ---------------------

void main() {
    uvec3 id = gl_GlobalInvocationID;

    uint w = uint(chunkWidth / precision);
    uint h = uint(chunkHeight / precision);

    if (id.x >= w || id.y >= h || id.z >= w) {
        return;
    }

    int idx = int(id.x) * precision;
    int idy = int(id.y) * precision;
    int idz = int(id.z) * precision;

    int index = int(id.z) * int(h) * int(w) + int(id.y) * int(w) + int(id.x);

    float n = noiseSplitter(
        offset.x + float(idx),
        offset.y + float(idy),
        offset.z + float(idz)
    );

    vertices[index] = vec4(float(idx), float(idy), float(idz), n);
}
