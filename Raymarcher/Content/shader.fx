float resolution_x;
float resolution_y;
float4 spheres[5];
float4 colors[5];
float3 position_offset;
float max_steps;
float u_elapsedTime;
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
	float closestDistance = 9999999;
	float steps_taken = 0.0;
	int closestSphere = -1;
	bool hit = false;
	float4 color;
	
	while(distanceToOrigin < 999)
	{
		float distance = 9999999;
		for (int i = 0; i < 5; i++) 
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
			if (cDistance < closestDistance && (cDistance < max_steps && (closestSphere == i || closestSphere == -1)))// || closestSphere == -1))
			{
				closestDistance = cDistance;
				closestSphere = i;
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

				if (closestSphere != i) {
					float background_shading = 1 - closestDistance / max_steps;
					color = float4(
						lerp(colors[i].x * shading, background_shading * colors[closestSphere].x, 0.5),
						lerp(colors[i].y * shading, background_shading * colors[closestSphere].y, 0.5),
						lerp(colors[i].z * shading, background_shading * colors[closestSphere].z, 0.5),
						1
						);
				}

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
		float background_shading = 1 - closestDistance / max_steps;
		color = float4(
			background_shading * colors[closestSphere].x, 
			background_shading * colors[closestSphere].y, 
			background_shading * colors[closestSphere].z, 
			background_shading
		);
		if (background_shading < 0) 
		{
			color = float4(
				0.64* (steps_taken / max_steps),//,40/255.0
				0.45*(steps_taken / max_steps), //,28/255.0
				0.96*(steps_taken / max_steps), //,60/255.0
				1
				);
		}
	}
	return color;
}

technique Technique1
{
	pass Pass1 {
		pixelShader = compile ps_3_0 PixelShaderFunction();
	}
}
