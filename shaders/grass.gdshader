shader_type spatial;
render_mode cull_disabled;

float inverseLerp(float v, float minValue, float maxValue) {
  return (v - minValue) / (maxValue - minValue);
}

float remap(float v, float inMin, float inMax, float outMin, float outMax) {
  float t = inverseLerp(v, inMin, inMax);
  return mix(outMin, outMax, t);
}

// The MIT License
// Copyright © 2013 Inigo Quilez
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions: The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software. THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// https://www.youtube.com/c/InigoQuilez
// https://iquilezles.org/
//
// https://www.shadertoy.com/view/Xsl3Dl
vec3 hash( vec3 p ) // replace this by something better
{
	p = vec3(
        dot(p,vec3(127.1,311.7, 74.7)),
			  dot(p,vec3(269.5,183.3,246.1)),
			  dot(p,vec3(113.5,271.9,124.6)));

	return -1.0 + 2.0*fract(sin(p)*43758.5453123);
}

float easeOut(float x, float t) {
	return 1.0 - pow(1.0 - x, t);
}

mat3 rotateY(float theta) {
  float c = cos(theta);
  float s = sin(theta);
  return mat3(
      vec3(c, 0, s),
      vec3(0, 1, 0),
      vec3(-s, 0, c)
  );
}

mat3 rotateAxis(vec3 axis, float angle) {
    axis = normalize(axis);

    float s = sin(angle);
    float c = cos(angle);
    float oc = 1.0 - c;

    return mat3(
        vec3(oc*axis.x*axis.x + c,
             oc*axis.x*axis.y + axis.z*s,
             oc*axis.z*axis.x - axis.y*s),

        vec3(oc*axis.x*axis.y - axis.z*s,
             oc*axis.y*axis.y + c,
             oc*axis.y*axis.z + axis.x*s),

        vec3(oc*axis.z*axis.x + axis.y*s,
             oc*axis.y*axis.z - axis.x*s,
             oc*axis.z*axis.z + c)
    );
}

vec3 bezier(vec3 P0, vec3 P1, vec3 P2, vec3 P3, float t) {
  return (1.0 - t) * (1.0 - t) * (1.0 - t) * P0 +
         3.0 * (1.0 - t) * (1.0 - t) * t * P1 +
         3.0 * (1.0 - t) * t * t * P2 +
         t * t * t * P3;
}

float noise( in vec3 p )
{
  vec3 i = floor( p );
  vec3 f = fract( p );
	
	vec3 u = f*f*(3.0-2.0*f);

  return mix( mix( mix( dot( hash( i + vec3(0.0,0.0,0.0) ), f - vec3(0.0,0.0,0.0) ), 
                        dot( hash( i + vec3(1.0,0.0,0.0) ), f - vec3(1.0,0.0,0.0) ), u.x),
                   mix( dot( hash( i + vec3(0.0,1.0,0.0) ), f - vec3(0.0,1.0,0.0) ), 
                        dot( hash( i + vec3(1.0,1.0,0.0) ), f - vec3(1.0,1.0,0.0) ), u.x), u.y),
              mix( mix( dot( hash( i + vec3(0.0,0.0,1.0) ), f - vec3(0.0,0.0,1.0) ), 
                        dot( hash( i + vec3(1.0,0.0,1.0) ), f - vec3(1.0,0.0,1.0) ), u.x),
                   mix( dot( hash( i + vec3(0.0,1.0,1.0) ), f - vec3(0.0,1.0,1.0) ), 
                        dot( hash( i + vec3(1.0,1.0,1.0) ), f - vec3(1.0,1.0,1.0) ), u.x), u.y), u.z );
}

uniform vec3 BASE_COLOUR: source_color;
uniform vec3 TIP_COLOUR: source_color;
uniform vec3 C21: source_color;
uniform vec3 C22: source_color;
uniform float grass_height = 5.0; // random height range added on top of grass_min_height
uniform float grass_min_height = 1.0;
varying vec3 vColour;
varying vec4 vGrassData;

void vertex() {
	int GRASS_SEGMENTS = int(5);
	int GRASS_VERTICES = int(GRASS_SEGMENTS + 1) * 2;
	float GRASS_WIDTH = 0.3;
	float GRASS_HEIGHT = grass_height * abs(noise(MODEL_MATRIX[3].xyz)) + grass_min_height;
		
  vec3 instance_world_origin = MODEL_MATRIX[3].xyz; // stable per instance
  vec3 hashValue = hash(instance_world_origin);
	
	// grass rotation
  float angle = remap(hashValue.x, -1.0, 1.0, -PI, PI);
	
	int vertFB_ID = VERTEX_ID % (GRASS_VERTICES * 2);
	int vertID = vertFB_ID % GRASS_VERTICES;
	
  int xTest = vertID & 0x1;
  int zTest = (vertFB_ID >= GRASS_VERTICES) ? 1 : -1;
  float xSide = float(xTest);
  float zSide = float(zTest);
  float heightPercent = float(vertID - xTest) / (float(GRASS_SEGMENTS) * 2.0);
	
	float width = GRASS_WIDTH * (1.0 - heightPercent);
	float height = GRASS_HEIGHT;
	
	float x = (xSide-0.5) * width;
	float y = heightPercent * height;
	float z = 0.0;
	
	// Grass lean factor
  float windStrength = noise(vec3(instance_world_origin.xz * 0.05, 0.0) + TIME);
  float windAngle = 0.0;
  vec3 windAxis = vec3(cos(windAngle), 0.0, sin(windAngle));
  float windLeanAngle = windStrength * 1.5 * heightPercent;

  float randomAnimation = noise(vec3(instance_world_origin.xz, TIME * 4.0)) * (windStrength * 0.5 + 0.125);
  

  float leanFactor = remap(hashValue.y, -1.0, 1.0, -0.5, 0.5) + randomAnimation;
  
  // add the bezxier curve for bend
  vec3 p1 = vec3(0.0);
  vec3 p2 = vec3(0.0, 0.33, 0.0);
  vec3 p3 = vec3(0.0, 0.66, 0.0);
  vec3 p4 = vec3(0.0, cos(leanFactor), sin(leanFactor));
  vec3 curve = bezier(p1, p2, p3, p4, heightPercent);
  
  y = curve.y * height;
  z = curve.z * height;
  
  // Generate grass amtrix
  mat3 grassMat = rotateAxis(windAxis, windLeanAngle) * rotateY(angle);
    
  // Instance transform already sits on the planet surface, oriented along its radial up
  VERTEX = grassMat * vec3(x, y, z);

  // vColor = mix(BASE_COLOUR, TIP_COLOUR, heightPercent);
  // vec3 grassLocalNormal = grassMat * vec3(0.0, curve.yz);
  // vColour = vec3(grassLocalNormal);

  vec3 c1 = mix(BASE_COLOUR, TIP_COLOUR, heightPercent);
  vec3 c2 = mix(C21, C22, heightPercent);
  float noiseValue = noise(instance_world_origin * 0.1);
  vColour = mix(c1, c2, smoothstep(-1.0, 1.0, noiseValue));
  vGrassData = vec4(x, 0.0, 0.0, 0.0);
}

void fragment() {
	ALBEDO = mix(vColour * 0.75, vColour, smoothstep(0.125, 0.0, abs(vGrassData.x)));
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
