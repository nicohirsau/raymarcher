float resolution_x;
float resolution_y;
float4 spheres[4];
float4 colors[4];
float3 position_offset;
float max_steps;
//float3 parameters;
float2 rotation;

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


float4 PixelShaderFunction(float2 coords: TEXCOORD0) : COLOR0
{
	float3 rayPosition = position_offset;
	float3 origin = position_offset;
	float3 direction = normalize(
		float3(
		lerp(-0.5, 0.5, coords.x),
		lerp(-0.5, 0.5, coords.y),
		1
	));

	float3x3 rotX = AngleAxis3x3(rotation.x, float3(1, 0, 0));
	direction = mul(direction, rotX);
	float3x3 rotY = AngleAxis3x3(rotation.y, float3(0, 1, 0));
	direction = mul(direction, rotY);

	float distanceToOrigin = 0.0;
	float steps_taken = 0.0;
	bool hit = false;
	float4 color;
	
	while(distanceToOrigin < 999)
	{
		float distance = 9999999;
		for (int i = 0; i < 4; i++) 
		{
			float cDistance = length(
				float3(
					spheres[i].x - rayPosition.x,
					spheres[i].y - rayPosition.y,
					spheres[i].z - rayPosition.z
				)
			) - spheres[i].w;
			if (cDistance < distance) 
			{
				distance = cDistance;
			}
			if (cDistance < 0.1) 
			{
				float shading = 1.0 - steps_taken / max_steps;
				color = float4(
					colors[i].x * shading,
					colors[i].y * shading,
					colors[i].z * shading,
					1
				);
				hit = true;
				break;
			}
		}
		if (hit) {
			break;
		}
		rayPosition += direction * distance;
		steps_taken = steps_taken + 1.0;
		distanceToOrigin =  length(rayPosition - origin);
	}
	if (!hit) {
		float background_shading = 1 * (steps_taken / (max_steps));
		color = float4(background_shading * 0.64, background_shading * 0.45, background_shading * 0.96, 1);
	}
	return color;
}

technique Technique1
{
	pass Pass1 {
		pixelShader = compile ps_3_0 PixelShaderFunction();
	}
}
