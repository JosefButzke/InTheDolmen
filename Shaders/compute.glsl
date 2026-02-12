#[compute]
#version 450

layout(local_size_x = 8, local_size_y = 8, local_size_z = 8) in;

// A binding to the buffer we create in our script
layout(set = 0, binding = 0, std430) writeonly buffer OutBuf {
    vec4 data[];
} out_buf;

// The code we want to execute in each invocation
void main() {
    // gl_GlobalInvocationID.x uniquely identifies this invocation across all work groups
    out_buf.data[gl_GlobalInvocationID.x] = vec4(gl_GlobalInvocationID.x, gl_GlobalInvocationID.y, gl_GlobalInvocationID.z, 3.0);
}