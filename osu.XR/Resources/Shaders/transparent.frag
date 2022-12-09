#version 330 core
#define GAMMA 2.4

uniform bool useGamma;
lowp float toSRGB ( lowp float color ) {
    return color < 0.0031308 ? ( 12.92 * color ) : ( 1.055 * pow( color, 1.0 / GAMMA ) - 0.055 );
}

lowp vec4 toSRGB( lowp vec4 colour )
{
    return vec4( toSRGB( colour.r ), toSRGB( colour.g ), toSRGB( colour.b ), colour.a );
}

in vec2 uv;

out vec4 FragColor;

uniform sampler2D tex;
uniform bool solidPass;
uniform vec4 subImage;
uniform vec4 tint;

void main ()
{
	FragColor = texture( tex, uv * subImage.zw + subImage.xy ) * tint;
	if ( useGamma )
		FragColor = toSRGB( FragColor );

	if ( solidPass && FragColor.a != 1 )
        discard;
    if ( FragColor.a == 0 )
        discard;
} 