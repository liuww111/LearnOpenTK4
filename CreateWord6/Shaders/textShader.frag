#version 460 core
in vec2 TexCoords;
out vec4 color;

uniform sampler2D text;
uniform vec3 textColor;

void main()
{    
    
    vec4 sampled = vec4(1.0, 1.0, 1.0, texture(text, TexCoords).r);
    color = vec4(textColor, 1.0) * sampled;
    //color = vec4(texture(text, TexCoords).r, texture(text, TexCoords).g, texture(text, TexCoords).a, 1.0);
} 