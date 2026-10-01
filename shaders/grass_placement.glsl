#[compute]
#version 450

// Scatters grass blades over a patch of the planet and snaps each one onto the
// same isosurface the marching-cubes terrain is built from.

layout(local_size_x = 64, local_size_y = 1, local_size_z = 1) in;

// Output in MultiMesh buffer layout (TRANSFORM_3D + custom data): 16 floats per instance
layout(set = 0, binding = 0, std430) writeonly buffer Instances {
    float data[];
} instances;

layout(set = 0, binding = 1, std140) uniform Params {
    vec4 patch_center;      // xyz: unit direction from planet center (planet space), w: patch radius (arc length)
    vec4 tangent;           // xyz: tangent at patch center, w: terrain grid resolution
    vec4 bitangent;         // xyz: bitangent at patch center, w: iso level
    vec4 settings;          // x: instance count, y: seed
    mat4 planet_to_local;   // planet space -> grass node local space
} params;

#include "planet_noise.glslinc"

const float SEARCH_TOP = PLANET_RADIUS + 16.0;
const float SEARCH_BOTTOM = PLANET_RADIUS - 32.0;
const float SEARCH_STEP = 1.0;
const int BISECT_STEPS = 8;

uint pcg(uint v) {
    uint state = v * 747796405u + 2891336453u;
    uint word = ((state >> ((state >> 28u) + 4u)) ^ state) * 277803737u;
    return (word >> 22u) ^ word;
}

float rand01(inout uint state) {
    state = pcg(state);
    return float(state) / 4294967295.0;
}

// The terrain samples noiseSplitter on a grid of `resolution` spacing and marching cubes
// interpolates linearly between samples, so sample the field the same way instead of
// evaluating the raw noise (the highest octave is finer than the grid).
float density(vec3 p) {
    float res = params.tangent.w;
    vec3 g = p / res;
    vec3 i = floor(g);
    vec3 f = g - i;
    vec3 b = i * res;

    float c000 = noiseSplitter(b);
    float c100 = noiseSplitter(b + vec3(res, 0.0, 0.0));
    float c010 = noiseSplitter(b + vec3(0.0, res, 0.0));
    float c110 = noiseSplitter(b + vec3(res, res, 0.0));
    float c001 = noiseSplitter(b + vec3(0.0, 0.0, res));
    float c101 = noiseSplitter(b + vec3(res, 0.0, res));
    float c011 = noiseSplitter(b + vec3(0.0, res, res));
    float c111 = noiseSplitter(b + vec3(res, res, res));

    float x00 = mix(c000, c100, f.x);
    float x10 = mix(c010, c110, f.x);
    float x01 = mix(c001, c101, f.x);
    float x11 = mix(c011, c111, f.x);
    return mix(mix(x00, x10, f.y), mix(x01, x11, f.y), f.z);
}

void write_instance(uint idx, vec3 x, vec3 y, vec3 z, vec3 origin, float found) {
    uint o = idx * 16u;
    instances.data[o + 0u] = x.x; instances.data[o + 1u] = y.x; instances.data[o + 2u] = z.x; instances.data[o + 3u] = origin.x;
    instances.data[o + 4u] = x.y; instances.data[o + 5u] = y.y; instances.data[o + 6u] = z.y; instances.data[o + 7u] = origin.y;
    instances.data[o + 8u] = x.z; instances.data[o + 9u] = y.z; instances.data[o + 10u] = z.z; instances.data[o + 11u] = origin.z;
    instances.data[o + 12u] = found;
    instances.data[o + 13u] = 0.0;
    instances.data[o + 14u] = 0.0;
    instances.data[o + 15u] = 0.0;
}

void main() {
    uint idx = gl_GlobalInvocationID.x;
    if (idx >= uint(params.settings.x)) return;

    float iso = params.bitangent.w;
    vec3 center = params.patch_center.xyz;

    // Uniform point inside a geodesic disk around the patch center
    uint state = pcg(idx ^ pcg(uint(params.settings.y)));
    float arc = params.patch_center.w * sqrt(rand01(state));
    float theta = 6.28318530718 * rand01(state);
    float angle = arc / PLANET_RADIUS;
    vec3 side = cos(theta) * params.tangent.xyz + sin(theta) * params.bitangent.xyz;
    vec3 dir = normalize(cos(angle) * center + sin(angle) * side);

    // March inward from above the terrain until we enter solid (density >= iso), then bisect
    float air_r = SEARCH_TOP;
    float solid_r = -1.0;
    for (float r = SEARCH_TOP - SEARCH_STEP; r >= SEARCH_BOTTOM; r -= SEARCH_STEP) {
        if (density(dir * r) >= iso) {
            solid_r = r;
            break;
        }
        air_r = r;
    }

    if (solid_r < 0.0) {
        // No surface along this ray: collapse the blade so it is not rendered
        vec3 origin = (params.planet_to_local * vec4(center * PLANET_RADIUS, 1.0)).xyz;
        write_instance(idx, vec3(0.0), vec3(0.0), vec3(0.0), origin, 0.0);
        return;
    }

    for (int i = 0; i < BISECT_STEPS; i++) {
        float mid = 0.5 * (air_r + solid_r);
        if (density(dir * mid) >= iso) {
            solid_r = mid;
        } else {
            air_r = mid;
        }
    }
    float surface_r = 0.5 * (air_r + solid_r);

    // Blade stands along the planet's radial up (same as gravity)
    vec3 origin = (params.planet_to_local * vec4(dir * surface_r, 1.0)).xyz;
    vec3 up = normalize(mat3(params.planet_to_local) * dir);
    vec3 ref = abs(up.y) < 0.99 ? vec3(0.0, 1.0, 0.0) : vec3(1.0, 0.0, 0.0);
    vec3 x = normalize(cross(up, ref));
    vec3 z = cross(x, up);
    write_instance(idx, x, up, z, origin, 1.0);
}
