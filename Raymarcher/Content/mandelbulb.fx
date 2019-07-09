float resolution_x;
float resolution_y;
float4 spheres[5];
float4 colors[5];
float3 position_offset;
float max_steps;
float u_elapsedTime;
//float3 parameters;
float2 rotation;
float2 resolution;
float Power = 2;
float Bailout = 1000;
int Iterations = 4;

float3x3 AngleAxis3x3(float angle, float3 axis)
{
	float c, s;
	sincos(angle, s, c);

	float t = 1 - c;
	float x = axis.x;
	float y = axis.y;
	float z = axis.z;

	return float3x3(
		t * x * x + c, t * x * y - s * z, t * x * z + s * y,
		t * x * y + s * z, t * y * y + c, t * y * z - s * x,
		t * x * z - s * y, t * y * z + s * x, t * z * z + c
	);
}


float GetDistanceToMandelbulb(float3 p)
{
	float3 c = p;
	float r = length(c);
	float dr = 1;
	for (int i = 0; i < 4 && r < 3; ++i)
	{
		float xr = pow(r, 7);
		dr = 6 * xr * dr + 1;

		float theta = atan2(c.y, c.x) * 8;
		float phi = asin(c.z / r) * 8;
		r = xr * r;
		c = r * float3(cos(phi) * cos(theta), cos(phi) * sin(theta), sin(phi));

		c += p;
		r = length(c);
	}
	return 0.35 * log(r) * r / dr;
}


float4 PixelShaderFunction(float2 coords: TEXCOORD0) : COLOR0
{
	float3 rayPosition = position_offset;
	float3 origin = position_offset;
	float widthToHeight = resolution.y / resolution.x;
	float3 direction = normalize(
		float3(
		lerp(-0.7, 0.7, coords.x),
		lerp(-0.7 * widthToHeight, 0.7 * widthToHeight, coords.y),
		1
	));

	float3x3 rotX = AngleAxis3x3(rotation.x, float3(1, 0, 0));
	direction = mul(direction, rotX);
	float3x3 rotY = AngleAxis3x3(rotation.y, float3(0, 1, 0));
	direction = mul(direction, rotY);

	float distanceToOrigin = 0.0;
	float closestDistance = 9999999;
	float steps_taken = 0.0;
	int firstClosestSphere = -1;
	bool hit = false;
	float4 color;
	
	while(distanceToOrigin < 1000)
	{
		float distance = GetDistanceToMandelbulb(rayPosition);
		//float distance = sin(rayPosition.xy);

		if (distance < 0.0001) {
			hit = true;
			color = float4(
				0.64 * (1-(steps_taken / max_steps)), //,40/255.0
				0.45 * (1-(steps_taken / max_steps)), //,28/255.0
				0.96 * (1-(steps_taken / max_steps)), //,60/255.0
				1
				);
			break;
		}
		rayPosition += direction * distance;
		steps_taken++;
		distanceToOrigin =  length(rayPosition - origin);
	}
	if (!hit) {
		color = float4(
			0.64 * (steps_taken / max_steps), //,40/255.0
			0.45 * (steps_taken / max_steps), //,28/255.0
			0.96 * (steps_taken / max_steps), //,60/255.0
			1
		);
	}
	return color;
}

technique Technique1
{
	pass Pass1 {
		pixelShader = compile ps_3_0 PixelShaderFunction();
	}
}
