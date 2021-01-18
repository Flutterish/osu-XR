#version 330 core
#define GAMMA 2.4

uniform bool useGammaCorrection;

lowp float toSRGB(lowp float color)
{
    return color < 0.0031308 ? (12.92 * color) : (1.055 * pow(color, 1.0 / GAMMA) - 0.055);
}

lowp vec4 toSRGB(lowp vec4 colour)
{
    return vec4(toSRGB(colour.r), toSRGB(colour.g), toSRGB(colour.b), colour.a);
}

in vec2 uv;

uniform sampler2D tx;
//uniform vec4 tint;

void main() 
{
    gl_FragColor = useGammaCorrection ? toSRGB( texture( tx, uv ) ) : texture( tx, uv );// * tint;
    //gl_FragColor = vec4(vec3(gl_FragCoord.z), 1.0);
}