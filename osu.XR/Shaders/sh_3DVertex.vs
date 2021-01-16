#version 330 core

attribute vec3 vertex;
attribute vec2 UV;

uniform mat4 worldToCamera;
uniform mat4 cameraToClip;
uniform mat4 localToWorld;

out vec2 uv;
out float z;

void main()
{
    uv = UV;
    vec4 temp = cameraToClip * worldToCamera * localToWorld * vec4( vertex, 1.0f );
    z = temp.z / temp.w;
    gl_Position = temp;
}