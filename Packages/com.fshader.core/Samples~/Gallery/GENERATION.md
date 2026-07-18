# Texture generation record

- Date: 2026-07-18
- Generator: Codex built-in `image_gen` (`imagegen` skill default mode)
- Generated source size: 1254 x 1254 RGB PNG
- Unity import size: maximum 1024; NPOT scaling and platform import may resize the source
- Shared constraints: square, seamless/tileable on all four edges, no perspective, no text, no logo, no watermark, no border

## Final prompt set

### Water_Base_Calm.png

Seamless top-down clear shallow water Base Map with subtle flowing turquoise and deep-cyan variation, neutral diffuse reference, broad soft flow bands and subtle small ripples, medium-low contrast; no foam, glare, caustics, shoreline, horizon, objects, shadows, or isolated focal feature.

### Water_WaveNormal_Fine.png

Seamless OpenGL-style tangent-space RGB normal map for fine overlapping wind-driven water ripples, suitable for sampling twice in different directions; predominantly `#8080ff`, subtle cyan/magenta slope deviations, low-to-moderate amplitude, no large swells or photographic shading.

### Water_FoamMask.png

Seamless technical grayscale water-foam mask with a pure-black background, thin interconnected white/light-gray foam filaments, broken bubbles and small clusters, with ample negative space; red channel usable as mask, no broad blobs or cresting waves.

### Ice_Base_Glacial.png

Seamless top-down pale-blue glacial ice Base Map with cloudy inclusions, layered frozen depth and fine crystalline grain, neutral diffuse reference, low-to-medium contrast; cracks extremely subtle because a separate crack mask controls them, no glare, debris, snow objects, or black fissures.

### Ice_Normal_Crystal.png

Seamless OpenGL-style tangent-space RGB normal map encoding fine crystalline ice grain and shallow frozen ridges; predominantly `#8080ff` with restrained cyan/magenta slopes, low amplitude, no deep cracks, photographic ice color, lighting, or large sharp facets.

### Ice_FrostMask.png

Seamless grayscale frost mask with soft cloudy frozen patches and fine feathery crystalline edges; black means clear ice and white means dense frost, balanced coverage near 45 percent, red channel usable as mask, no symbolic snowflakes or hard geometric cells.

### Ice_CrackMask.png

Seamless grayscale ice-crack detail mask with a pure-black background, sparse thin white/gray irregular branches and hairline offshoots, no central impact point, mostly negative space; red channel usable as mask, no dense spiderweb or shattered-glass polygons.

### Glass_Condensation_RGB.png

Seamless technical RGB packed condensation control map on black: red channel contains small and medium droplets, green contains sparse thin vertical trails, blue contains broad soft micro-fog patches; features remain independently readable, with no photographed glass, scene, or opaque full-frame fog.

### Glass_CondensationNormal.png

Seamless OpenGL-style tangent-space RGB normal map for mostly flat glass with many small rounded water droplets, a few medium droplets and sparse thin vertical runnels; predominantly `#8080ff`, moderate local relief, no fog color, dark background, perspective, or isolated hero droplet.
