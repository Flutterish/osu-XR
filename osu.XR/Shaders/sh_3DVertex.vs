#version 330 core

attribute vec3 vertex;
attribute vec2 UV;

uniform mat4 worldToCamera;
uniform mat4 cameraToClip;
uniform mat4 localToWorld;

out vec2 uv;

void main()
{
    uv = UV;
    gl_Position = cameraToClip * worldToCamera * localToWorld * vec4( vertex, 1.0f );
}