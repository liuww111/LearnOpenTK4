#version 330 core
in vec2 TexCoords;
out vec4 color;

uniform sampler2D texture0;

void main()
{    
    //color =  vec4(1.0, 1.0, 1.0, texture(texture0, TexCoords).r);
    color = texture(texture0, TexCoords);
} 