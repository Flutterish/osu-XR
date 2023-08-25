#version 330 core
in vec2 uv;

out vec4 FragColor;

uniform sampler2D tex;
uniform bool solidPass;

void main()
{
    FragColor = texture( tex, vec2(uv.x, 1 - uv.y) );
    if ( solidPass && FragColor.a != 1 )
        discard;
    if ( FragColor.a == 0 )
        discard;
} 