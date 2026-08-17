# Short-Form Ad Video Pattern Library — Context File for Gemini API

> Hợp nhất từ 5 lượt research độc lập (TikTok Creative Center, UGC ad guides, Runway/Kling/Veo/Sora/Luma prompting docs). Đã loại trùng lặp, giữ bản chi tiết nhất cho mỗi pattern, chuẩn hoá field. File này dùng làm **system context** gửi kèm mỗi lần gọi Gemini API để generate prompt video quảng cáo — AI đọc toàn bộ thư viện, chọn pattern khớp với sản phẩm/ngành hàng hiện tại, rồi viết prompt theo format T2VA/full-reference đã định nghĩa riêng.

---

## PART 1 — Pattern Library (26 patterns)

---
PATTERN_ID: ugc_testimonial_arc
NAME: UGC Testimonial / Transformation Arc
BEST_FOR: beauty, fitness/wellness, tech, home goods, services/apps, B2B
HOOK_STRATEGY: Creator opens mid-sentence with a specific struggle, skepticism, or result claim in 1-2s ("I was stuck at the same weight for 3 months") — peer-level recognition lowers ad-defense filters.
SHOT_FLOW: Shot 1: talking-head states struggle/skepticism -> Shot 2: brief context on the problem -> Shot 3: product discovery/introduction mid-speech -> Shot 4: use/application with natural reaction -> Shot 5: specific measurable result -> Shot 6: recommendation + CTA
CAMERA_NOTES: Handheld phone-style, natural micro-shake, eye-level medium close-up, cuts every 2-4s, no polish.
AUDIO_NOTES: Direct creator voiceover, conversational pace, natural ambient room tone, light or no music, captions essential.
AVOID: Polished studio talking-heads or generic praise without a specific struggle/measurable result — reads as scripted, not testimonial.
---

---
PATTERN_ID: problem_solution_interrupt
NAME: Problem-Agitate-Solve (PAS) Direct
BEST_FOR: beauty, tech, home goods, fitness/wellness, services/apps, B2B
HOOK_STRATEGY: Vivid, hyper-specific frustration shown/stated in 0-2s, before any brand mention — forces immediate self-recognition.
SHOT_FLOW: Shot 1: problem moment in real context -> Shot 2: agitate/consequence of the problem -> Shot 3: product enters mid-action as the fix -> Shot 4: rapid resolution demo -> Shot 5: payoff reaction/measurable outcome -> Shot 6: CTA
CAMERA_NOTES: Fast cuts (1.5-3s), mix of handheld POV and static inserts, hard visual contrast between problem/solution states.
AUDIO_NOTES: Voiceover or on-camera speech describing the problem conversationally; SFX marks the transition; music builds through the solution.
AVOID: Opening with brand/product before the problem is established — hook must be problem-first.
---

---
PATTERN_ID: unboxing_reveal
NAME: Unboxing / Sensory Reveal
BEST_FOR: beauty, food/beverage, fashion, tech, home goods
HOOK_STRATEGY: First hand-on-packaging movement, tear, or pour fills frame 0 — sensory curiosity and dopamine-hit reveal.
SHOT_FLOW: Shot 1: hands on sealed package/first tear -> Shot 2: quick cut through packaging layers -> Shot 3: product reveal with genuine reaction -> Shot 4: macro texture/detail touch -> Shot 5: first use/impression -> Shot 6: verdict + CTA
CAMERA_NOTES: Close-up/macro on hands and product, static or slight handheld, fast cuts through packaging, slow-down on the reveal beat.
AUDIO_NOTES: ASMR-forward (crinkle, tape pull, pour, click), minimal or restrained voiceover, little to no music.
AVOID: Long unsealing sequences with dead time, or showing the full product too early — cut to the satisfying moments.
---

---
PATTERN_ID: lifestyle_context_integration
NAME: Lifestyle Product-in-Context
BEST_FOR: fashion, food/beverage, fitness/wellness, home goods, beauty, tech
HOOK_STRATEGY: Viewer dropped mid-scene into an aspirational or everyday moment where the product is already in natural use — desire through context, not claim.
SHOT_FLOW: Shot 1: lifestyle scene establishing context -> Shot 2: product enters organically within the activity -> Shot 3: short context cuts showing versatility/benefit in situ -> Shot 4: emotional/social payoff -> Shot 5: soft CTA
CAMERA_NOTES: Handheld documentary or gimbal-stabilized movement through space, 2-4s cuts, natural/golden-hour light, occasional detail insert.
AUDIO_NOTES: Ambient environmental sound + light music or trending audio, optional soft VO, no hard sales pitch.
AVOID: Isolated studio hero shots with no human/environmental context — product must feel lived-in.
---

---
PATTERN_ID: macro_detail_texture
NAME: Macro Detail & Texture Showcase
BEST_FOR: beauty, food/beverage, fashion, tech, home goods
HOOK_STRATEGY: Extreme close-up of texture, liquid, or material fills frame 1 — visual fascination and tactile curiosity before any wider reveal.
SHOT_FLOW: Shot 1: macro texture/material in motion -> Shot 2: slow pull-back or orbit revealing full product -> Shot 3: application/interaction detail -> Shot 4: result on skin/surface/in-use -> Shot 5: product hero + CTA
CAMERA_NOTES: Locked or very slow macro moves, shallow depth of field, controlled rim lighting, minimal cuts/long takes.
AUDIO_NOTES: Soft ASMR or amplified tactile sound (drip, click, rustle), restrained or no music.
AVOID: Wide static catalog-style shots, or macro that shows appearance but never utility/benefit.
---

---
PATTERN_ID: dynamic_action_demo
NAME: Dynamic Action / Motion Demonstration
BEST_FOR: fitness/wellness, tech, fashion, food/beverage, home goods
HOOK_STRATEGY: Product/person already mid-motion delivering visible output in frame 0-1.5s — proves capability before naming the product.
SHOT_FLOW: Shot 1: product mid-action delivering clear result -> Shot 2: wider context of the action -> Shot 3: quick feature callouts via cuts -> Shot 4: reaction or secondary benefit -> Shot 5: CTA with product still active
CAMERA_NOTES: Energetic handheld/tracking, rapid cuts (1-2.5s), dynamic angles, one purposeful camera move per shot, speed-ramp for contrast.
AUDIO_NOTES: High-energy music matching action tempo, sound design on impact beats, minimal/no voiceover — action speaks.
AVOID: Static or slow-paced shots mismatched to the category's natural energy; random camera movement unrelated to the action.
---

---
PATTERN_ID: before_after_transformation
NAME: Before-After Transformation
BEST_FOR: beauty, fitness/wellness, home goods, food/beverage, services/apps
HOOK_STRATEGY: Clear "before" deficiency shown immediately, or the final state teased first — opens a loop the "after" pays off within 1-2s.
SHOT_FLOW: Shot 1: before state in real context -> Shot 2: transition/process indication -> Shot 3: product application/action -> Shot 4: after reveal, matched framing/lighting -> Shot 5: close proof detail or timeframe text -> Shot 6: CTA
CAMERA_NOTES: Matched framing/lighting across before/after for credibility, match cuts or wipes, locked or repeatable camera position.
AUDIO_NOTES: VO narrating the change or text-only, transition whoosh/beat drop at reveal, light music.
AVOID: Mismatched lighting/angles that undermine credibility; vague improvement claims with no visual proof; missing timeframe context.
---

---
PATTERN_ID: side_by_side_comparison
NAME: Side-by-Side / Split-Screen Comparison
BEST_FOR: tech, beauty, fashion, home goods, B2B, fitness
HOOK_STRATEGY: Two options (product vs. alternative/competitor/old-way) appear together in 1-2s with a "which wins" framing — immediate decision frame.
SHOT_FLOW: Shot 1: both items/states in frame with comparison claim -> Shot 2: same test applied to both -> Shot 3: visible difference highlight -> Shot 4: winner declaration with reason -> Shot 5: CTA
CAMERA_NOTES: Split-screen or alternating cuts, consistent angle/lighting/timing across both sides, text callouts for labels.
AUDIO_NOTES: Direct VO explaining the difference, synced SFX per side's action, neutral/minimal music.
AVOID: Vague superiority claims with no visual proof; visibly unfair test setup.
---

---
PATTERN_ID: sensory_first_reaction
NAME: Sensory First-Bite / First-Touch Reaction
BEST_FOR: food/beverage, beauty (texture), fitness/wellness (supplements)
HOOK_STRATEGY: Extreme close-up of product entering mouth/skin, or the immediate facial reaction, fills 1-2s — mirror-neuron craving/curiosity.
SHOT_FLOW: Shot 1: product-to-mouth/skin or pure reaction close-up -> Shot 2: wider shot of person + product -> Shot 3: verbal/text description of sensation -> Shot 4: secondary benefit/continued enjoyment -> Shot 5: CTA
CAMERA_NOTES: Tight close-ups, handheld or slight push-in, rapid reaction cut.
AUDIO_NOTES: Crunch/sip/pour ASMR dominant, authentic reaction sound/VO, minimal music.
AVOID: Delayed product introduction after a talking setup — loses sensory immediacy.
---

---
PATTERN_ID: try_on_haul_transition
NAME: Try-On Haul Transition
BEST_FOR: fashion, beauty (makeup), home goods (decor)
HOOK_STRATEGY: Quick "just got this" statement or package reveal followed by immediate on-body/in-space transition within 2s.
SHOT_FLOW: Shot 1: package/pile of items + excited intro -> Shot 2: rapid outfit/product changes with transitions -> Shot 3: fit/detail close-ups and honest reaction -> Shot 4: styling versatility/multiple angles -> Shot 5: keep/return verdict + CTA
CAMERA_NOTES: Full-body and detail mix, mirror or static camera, jump-cut transitions, vertical framing.
AUDIO_NOTES: Creator VO commentary, trending/upbeat audio, occasional ASMR of fabric/zip.
AVOID: Static flat-lay-only presentation with no body/context movement.
---

---
PATTERN_ID: screen_recording_walkthrough
NAME: Screen-Recording App Walkthrough
BEST_FOR: services/apps, B2B, tech (software features)
HOOK_STRATEGY: Immediate screen recording of a clear before-state problem (messy dashboard, slow process) or surprising output within 1-2s.
SHOT_FLOW: Shot 1: problem screen/before UI state -> Shot 2: app interaction begins -> Shot 3: step-by-step feature demo with cursor/finger highlight -> Shot 4: transformed result screen -> Shot 5: download/CTA overlay
CAMERA_NOTES: Clean screen capture with subtle zoom/pan on key UI, occasional picture-in-picture face reaction.
AUDIO_NOTES: Clear VO per step, UI click/swoosh SFX, light or no music.
AVOID: Pure talking-head feature lists without showing the interface in action.
---

---
PATTERN_ID: myth_vs_fact
NAME: Myth-Bust / Myth vs Fact Education
BEST_FOR: beauty, fitness/wellness, B2B, tech, services/apps
HOOK_STRATEGY: Bold contrarian statement or named misconception in 1-2s ("You need 10 steps for clear skin") — cognitive dissonance, positions brand as authority.
SHOT_FLOW: Shot 1: myth statement on screen/spoken -> Shot 2: fact reveal/correction -> Shot 3: product introduced as the correct approach -> Shot 4: proof or mechanism explanation -> Shot 5: CTA framed as the smarter choice
CAMERA_NOTES: Talking-head with B-roll inserts, confident framing, text overlays for myth/fact labels.
AUDIO_NOTES: Authoritative or energetic VO, minimal music so the claim lands cleanly.
AVOID: Presenting myth as fact, or staying vague instead of a clear wrong-way-vs-right-way contrast.
---

---
PATTERN_ID: overhead_process_build
NAME: Overhead Process Build
BEST_FOR: food/beverage, home goods, beauty (routine), fitness (prep)
HOOK_STRATEGY: Top-down view of ingredients/tools already in motion (pour, mix, assemble) in second 1 — hypnotic process curiosity.
SHOT_FLOW: Shot 1: overhead process start with key action -> Shot 2: sequential steps with text labels -> Shot 3: transformation midpoint -> Shot 4: finished result reveal -> Shot 5: branding + CTA
CAMERA_NOTES: Locked or slow-moving overhead/flat-lay, clean cuts or continuous process, high contrast lighting for ingredients.
AUDIO_NOTES: Satisfying process sounds (sizzle, pour, mix), optional short VO or text-only, light rhythmic music.
AVOID: Talking-head recipe intros that delay the visual process.
---

---
PATTERN_ID: result_first_payoff
NAME: Result-First Payoff / Value-Demo-First
BEST_FOR: beauty, fitness/wellness, tech, home goods, services/apps
HOOK_STRATEGY: The finished desirable outcome appears in frame 1, then the video shows how it was achieved — no logo, no intro, immediate value.
SHOT_FLOW: Shot 1: pure result/hero outcome visual -> Shot 2: brief "here's how" transition -> Shot 3: condensed process/product use -> Shot 4: return to result with social proof -> Shot 5: CTA
CAMERA_NOTES: Clean/high-production result shot first, faster cuts for process, strong visual contrast between result and process.
AUDIO_NOTES: Satisfying result SFX/music sting on open, VO explaining the path, upbeat close.
AVOID: Process-heavy opens that bury the benefit past the 3-second scroll-decision window; starting with brand intro before demonstrating function.
---

---
PATTERN_ID: founder_story_bts
NAME: Founder Story / Behind-the-Scenes
BEST_FOR: beauty, food/beverage, home goods, fashion, services/apps, B2B
HOOK_STRATEGY: Founder on-camera or BTS creation footage opens — humanizes brand, builds trust through authenticity.
SHOT_FLOW: Shot 1: founder on-camera or BTS of product being made -> Shot 2: problem founder experienced that led to the product -> Shot 3: development/creation moment -> Shot 4: product in use or result -> Shot 5: CTA with founder recommendation
CAMERA_NOTES: Handheld/static phone-camera feel, natural lighting, 2-4s cuts, unpolished aesthetic.
AUDIO_NOTES: Founder VO or direct address, natural ambient tone, minimal/no music.
AVOID: Overly polished or scripted founder segments — must feel personal, not corporate.
---

---
PATTERN_ID: grwm_routine
NAME: GRWM (Get Ready With Me) / Routine Walkthrough
BEST_FOR: beauty, fashion, fitness/wellness, home goods, food/beverage
HOOK_STRATEGY: Creator starts a recognizable routine (morning, workout prep, meal prep) — viewer expects the full process and natural product integration.
SHOT_FLOW: Shot 1: creator starting routine -> Shot 2: product introduced as a step -> Shot 3: application/use in context -> Shot 4: result/benefit of that step -> Shot 5: routine continuation/final look -> Shot 6: CTA
CAMERA_NOTES: Handheld/static phone-camera, 2-4s cuts per step, natural lighting, eye-level framing.
AUDIO_NOTES: Creator VO explaining steps, light background music, captions for mute viewing.
AVOID: Skipping steps or showing product with no context — must feel like an authentic routine, not forced placement.
---

---
PATTERN_ID: skeptic_to_fan
NAME: Skeptic-to-Fan Transformation
BEST_FOR: beauty, fitness/wellness, tech, services/apps, home goods, food/beverage
HOOK_STRATEGY: Opens with skepticism ("I tried four others first") — builds credibility through honesty before the pivot.
SHOT_FLOW: Shot 1: creator states skepticism/failed attempts -> Shot 2: context on why they were skeptical -> Shot 3: product discovery moment -> Shot 4: turning point/first positive result -> Shot 5: specific measurable outcome -> Shot 6: CTA
CAMERA_NOTES: Handheld/static phone-camera, 2-3s cuts, authentic UGC aesthetic.
AUDIO_NOTES: Conversational VO, minimal music, captions essential.
AVOID: Fake enthusiasm or instant conversion — needs a genuine skepticism beat and a credible turning point.
---

---
PATTERN_ID: proof_stack_social
NAME: Proof Stack / Social Proof Montage
BEST_FOR: services/apps, B2B, tech, beauty, fitness/wellness, home goods
HOOK_STRATEGY: Opens with a stat/headline ("10,000+ users", "4.9 stars") or one striking customer reaction, then stacks more voices — credibility fast.
SHOT_FLOW: Shot 1: social proof headline/stat or strongest customer reaction -> Shot 2: second customer/use case -> Shot 3: third customer/use case -> Shot 4: product shown in context -> Shot 5: recurring benefit/theme -> Shot 6: CTA
CAMERA_NOTES: Mix of creator selfies, customer clips, screenshots, product footage; keep individual clips short and visually varied.
AUDIO_NOTES: Natural voices stitched together, captions, light music; vary delivery so testimonials don't sound identical.
AVOID: Unsupported/vague stats; repeating generic praise ("amazing!") without specifics.
---

---
PATTERN_ID: pattern_interrupt_visual
NAME: Universal Pattern Interrupt Open
BEST_FOR: all categories — universal hook mechanism, especially useful when no other pattern fits cleanly
HOOK_STRATEGY: Opens with an unexpected visual, abrupt motion, or contradiction to category norm that breaks scrolling rhythm.
SHOT_FLOW: Shot 1: pattern interrupt (unexpected visual/motion/statement) -> Shot 2: reveal of product context or problem -> Shot 3: product introduced/demo -> Shot 4: proof or benefit -> Shot 5: CTA
CAMERA_NOTES: Varies by interrupt type — fast zoom, snap cut, abrupt setting change, or striking static image; 1-2s cuts through the hook.
AUDIO_NOTES: SFX on the interrupt moment (whoosh, snap, drop), music/VO after the hook.
AVOID: Generic or predictable openings; a shocking hook that never bridges into the product value (high hook rate but no conversion).
---

---
PATTERN_ID: curiosity_progressive_reveal
NAME: Curiosity → Progressive Reveal
BEST_FOR: beauty, tech, fashion, food/beverage, home goods, services/apps
HOOK_STRATEGY: Intriguing visual, unusual action, or unexplained situation opens; explanation is delayed just long enough to create a reason to keep watching.
SHOT_FLOW: Shot 1: unexplained visual/question -> Shot 2: additional clue -> Shot 3: product/context partially revealed -> Shot 4: mechanism/use revealed -> Shot 5: payoff -> Shot 6: CTA
CAMERA_NOTES: Unusual close-up or unexpected composition to start, gradually widening; each cut answers one question while raising another.
AUDIO_NOTES: Suspenseful/curiosity-building sound, short VO questions, timed silence, then payoff sound/music.
AVOID: Withholding information so long the viewer gets confused rather than curious.
---

---
PATTERN_ID: scenario_specific_customization
NAME: Scenario → Personalized Fit
BEST_FOR: fashion, beauty, services/apps, B2B, home goods
HOOK_STRATEGY: Opens on a highly specific person/situation/constraint so the target viewer immediately recognizes themselves.
SHOT_FLOW: Shot 1: specific persona/situation -> Shot 2: unique need or constraint -> Shot 3: product/service customized to that situation -> Shot 4: result -> Shot 5: proof/detail -> Shot 6: CTA
CAMERA_NOTES: Contextual medium shots combined with detail shots showing the customization; follows the scenario rather than defaulting to studio footage.
AUDIO_NOTES: Persona-specific dialogue/VO reflecting the target user's exact situation.
AVOID: Broad "this is for everyone" messaging — specificity is the entire mechanism.
---

---
PATTERN_ID: mechanism_of_action_explainer
NAME: Mechanism of Action (MoA) Explainer
BEST_FOR: B2B, tech, beauty, fitness/wellness, services/apps
HOOK_STRATEGY: Visualizes an invisible process or complex workflow within 2s using a clean cross-section/schematic view — makes the abstract concrete fast.
SHOT_FLOW: Shot 1: cross-section/schematic view showing the internal friction/problem -> Shot 2: animated flow of the solution moving through the system -> Shot 3: pull-back to the real-world interface or polished exterior -> Shot 4: benefit statement -> Shot 5: CTA
CAMERA_NOTES: Smooth orbit or slow crane move descending into the internal core of the product/process.
AUDIO_NOTES: Calm, authoritative explanatory VO, subtle futuristic UI audio cues.
AVOID: Overly abstract technical visuals that obscure the pragmatic user benefit.
---

---
PATTERN_ID: street_interview_vox_pop
NAME: Street Interview Vox-Pop Reactions
BEST_FOR: services/apps, food/beverage, fashion, fitness/wellness, tech
HOOK_STRATEGY: Opens mid-conversation with an interviewer asking a provocative question in a public setting — raw, unscripted energy.
SHOT_FLOW: Shot 1: interviewer asks a fast-paced question on a busy street -> Shot 2: unfiltered passerby reaction testing/inspecting the product -> Shot 3: group laugh or consensus statement confirming value -> Shot 4: CTA
CAMERA_NOTES: Handheld, natural zoom/pans between interviewer and interviewee, raw ambient lighting.
AUDIO_NOTES: Street ambience mixed with clear vocal capture — authentic broadcast feel.
AVOID: Rehearsed actor responses or studio-cleaned audio that breaks the "real street" illusion.
---

---
PATTERN_ID: timeline_tension_durability
NAME: Timeline Tension / Stress Test
BEST_FOR: tech, home goods, fashion, fitness/wellness, B2B
HOOK_STRATEGY: Visual countdown or durability milestone ("30 days straight") establishes narrative tension immediately.
SHOT_FLOW: Shot 1: stress test/time-lapse setup mid-action -> Shot 2: fast-forward sequence under continuous strain -> Shot 3: close-up inspection revealing unblemished condition -> Shot 4: CTA
CAMERA_NOTES: Locked static angle for time-lapse, dynamic slow-motion tracking on high-impact moments.
AUDIO_NOTES: Ticking clock or rising-pitch tension audio building to the outcome reveal.
AVOID: Unbelievable/artificial physics that damage viewer trust in the claim.
---

---
PATTERN_ID: subverted_expectation_reverse
NAME: Reverse Psychology / Subverted Expectation
BEST_FOR: beauty, fashion, tech, services/apps, B2B
HOOK_STRATEGY: Counter-intuitive statement ("Don't buy this if you want X") paired with visual contradiction — intense curiosity gap.
SHOT_FLOW: Shot 1: bold counter-intuitive statement with contradictory action -> Shot 2: rapid montage of unexpected benefits -> Shot 3: playful wrap-up reinforcing the core value prop -> Shot 4: CTA
CAMERA_NOTES: Tight medium close-up on the speaker, rapid push-in zoom on the hook line.
AUDIO_NOTES: Deadpan confident delivery, complete silence or music-drop on the hook line.
AVOID: Overly aggressive sales pitches that immediately signal commercial intent and undercut the reversal.
---

## PART 2 — Cross-Category Applicability Matrix

Scale: **H** = strong native fit, **M** = viable secondary fit, blank = weak/rarely used

| PATTERN_ID | Beauty | Food/Bev | Fashion | Tech | Home Goods | Fitness | Services/Apps | B2B |
|---|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| ugc_testimonial_arc | H | M | H | M | H | H | H | M |
| problem_solution_interrupt | H | M | M | H | H | H | H | H |
| unboxing_reveal | H | H | H | H | H | M | | |
| lifestyle_context_integration | H | H | H | M | H | H | M | |
| macro_detail_texture | H | H | H | H | H | M | | |
| dynamic_action_demo | M | H | H | H | M | H | M | |
| before_after_transformation | H | M | M | H | H | H | H | M |
| side_by_side_comparison | H | M | H | H | H | M | H | H |
| sensory_first_reaction | M | H | | | | M | | |
| try_on_haul_transition | H | | H | | M | | | |
| screen_recording_walkthrough | | | | H | | M | H | H |
| myth_vs_fact | H | M | | H | | H | H | H |
| overhead_process_build | M | H | | | H | M | | |
| result_first_payoff | H | M | M | H | H | H | H | M |
| founder_story_bts | H | H | H | | H | | H | H |
| grwm_routine | H | M | H | | H | H | | |
| skeptic_to_fan | H | M | | H | H | H | H | |
| proof_stack_social | H | | | H | H | H | H | H |
| pattern_interrupt_visual | H | H | H | H | H | H | H | H |
| curiosity_progressive_reveal | H | H | H | H | M | M | H | |
| scenario_specific_customization | H | | H | H | H | | H | H |
| mechanism_of_action_explainer | H | | | H | M | H | H | H |
| street_interview_vox_pop | M | H | H | M | | H | H | |
| timeline_tension_durability | | | H | H | H | H | M | H |
| subverted_expectation_reverse | H | M | H | H | M | H | H | H |

## PART 3 — Anti-Patterns (apply globally, regardless of chosen pattern)

- **PLAIN_PRODUCT_ROTATION** — a product simply spinning on a plain studio background with no human context, texture, or motion beyond the spin. Reads as catalog footage, not native content. This is the default failure mode to actively avoid.
- **STATIC_STUDIO_ONLY** — fully polished studio presentation with zero human context, action, or narrative.
- **BRAND_FIRST_INTRO** — logo, company name, or generic brand statement before the hook lands. Brand comes after relevance is established, not before.
- **FEATURE_DUMP** — listing multiple features verbally without showing what each one does or its consequence for the user.
- **GENERIC_STOCK_LIFESTYLE** — interchangeable lifestyle footage that could sell any competing product; must expose something product-specific.
- **OVER_POLISHED_CORPORATE_UGC** — "UGC" label slapped on obviously scripted, perfectly framed corporate spokesperson footage.
- **RANDOM_CINEMATIC_CAMERA** — arbitrary pans/orbits/zooms added only to look "cinematic," unrelated to subject action. Camera motion should always support the action; one clear primary movement per shot.
- **OVERLOADED_SINGLE_PROMPT** — cramming many unrelated actions, camera moves, and scene changes into one shot/generation. Keep each shot to one clear beat.
- **HIGH_HOOK_NO_BRIDGE** — a shocking/viral-feeling hook that never bridges into the actual product value, causing high early retention but steep drop-off before the CTA.
- **IN_ENGINE_TEXT_RENDERING** — relying on the AI video model to render on-screen price tags, callouts, or promo text; text generated in-engine is prone to artifacting. Render clean video and add typography/captions in post instead.
- **VAGUE_CONCEPTUAL_PROMPTING** — abstract adjectives ("make it look luxurious", "innovative", "emotional") instead of concrete camera/action/lighting instructions; produces drift and inconsistency.
- **SILENT_OR_MUSIC_ONLY_OPEN** — no text overlay or clear visual action for sound-off viewers.
- **REPEATED_IDENTICAL_CREATIVE** — generating many near-identical ads with only superficial wording changes instead of genuinely varying the pattern.

## PART 4 — System Instruction for Gemini API Integration

Dán khối này làm `system_instruction` khi gọi Gemini API. Nó đọc trực tiếp PART 1-3 ở trên (gửi kèm làm context), tự chọn pattern theo sản phẩm, tự kiểm tra anti-pattern, nhưng **chỉ được xuất ra kịch bản T2VA cuối cùng — không có bất kỳ nội dung nào khác**. Bản này được tối ưu cho **TikTok product ad cực ngắn, 8-10 giây** — tức là script phải NÉN pattern gốc (vốn thiết kế 4-6 shot cho video 15s+) xuống còn 2-3 shot cốt lõi.

```
You are a video prompt generation engine for ultra-short TikTok product ads (8-10 seconds total runtime). You have access to a pattern library (provided as context in this request) containing 26 creative patterns with hook strategies, shot flows, camera/audio notes, and category fit ratings. The patterns were written assuming a 15-30s format with 4-6 shots — your job includes compressing the chosen pattern down to fit 8-10 seconds without losing its hook mechanism.

## OUTPUT CONTRACT — THIS OVERRIDES EVERYTHING ELSE BELOW
Your entire visible response must contain ONLY the final T2VA script. Nothing else is allowed, under any circumstance:
- Do NOT output which pattern you selected or why (no "Selected pattern: ...", no pattern name, no rating).
- Do NOT output any greeting, preamble, closing remark, disclaimer, or label — this includes phrases like "Here is your script", "Dưới đây là kịch bản...", "Kết quả:", "Hy vọng hữu ích", or any translation/explanation of the prompt.
- Do NOT wrap the output in markdown code fences, quotation marks, or any container.
- Do NOT add headings, numbering, or commentary between the three T2VA fields.
- The response MUST start on the very first character with `integrated_multimodal_description:` — nothing precedes it, not even a blank line.
- The response MUST end immediately after the `non_diegetic_music:` content — nothing follows it.
- All steps below (pattern selection, anti-pattern check, duration planning) are internal reasoning only. They inform what you write, but their reasoning, labels, or conclusions must never appear in the output text itself.

## Step 1 — Select a pattern (internal reasoning, never shown in output)
Given the product name, description, and target audience provided in the user message:
1. Identify the product's category (beauty, food/beverage, fashion, tech, home goods, fitness/wellness, services/apps, or B2B).
2. Consult the Cross-Category Applicability Matrix (Part 2) and shortlist patterns rated H (strong fit) for that category.
3. Among the shortlist, prefer patterns whose hook lands and whose value is legible within 1-2 shots — e.g., result_first_payoff, problem_solution_interrupt, macro_detail_texture, dynamic_action_demo, unboxing_reveal, side_by_side_comparison, sensory_first_reaction, pattern_interrupt_visual. Multi-beat narrative patterns (founder_story_bts, grwm_routine, skeptic_to_fan, proof_stack_social, try_on_haul_transition) are still valid choices when they fit the category best, but require harder compression in Step 3.
4. Choose ONE pattern from the shortlist. If a "recently used patterns" list is provided, exclude those from consideration to maintain variety across a batch.
5. Keep this decision entirely internal — it must never be printed, hinted at, or labeled in the visible output.

## Step 2 — Apply the anti-patterns (internal reasoning, never shown in output)
Cross-check your planned shot flow against Part 3. If it resembles PLAIN_PRODUCT_ROTATION, BRAND_FIRST_INTRO, or any other listed anti-pattern, revise the shot flow before finalizing. Do not mention this check in the output.

## Step 3 — Compress into an 8-10 second TikTok ad format
Total runtime across all shots combined must fall between 8 and 10 seconds — never shorter, never longer. Every pattern's SHOT_FLOW in Part 1 was written for a 15-30s format with 4-6 beats, so it must be compressed, not used as-is:
1. **Shot cap: 2-3 shots maximum.** Never use 4 or more shots — at 8-10s total, more shots means each one is too short (under ~2s) for an AI video model to render a legible action or camera move. Use 2 shots for patterns whose hook and payoff can share one visual idea (e.g., macro_detail_texture, unboxing_reveal, dynamic_action_demo); use 3 shots only when the pattern genuinely needs a separate beat for hook / product-in-use / payoff (e.g., before_after_transformation, problem_solution_interrupt).
2. **What to always keep:**
   - The Hook shot — always Shot 1, non-negotiable, this is what stops the scroll in the first ~1-2s.
   - A shot that shows the product delivering its core benefit/result — merge the pattern's "discovery", "application", and "result" beats into this single compressed shot.
   - A CTA moment — fold it into the final shot's action/text overlay rather than giving it a standalone shot, unless 3 shots are used and one is dedicated to the payoff.
3. **What to drop:** secondary context beats, extra reactions, multiple customer voices/use cases, styling variations, or any beat beyond hook → core benefit → payoff/CTA. An 8-10s ad has no room for them — cut, don't summarize.
4. **Typical timing:** 2 shots of ~4-5s each, or 3 shots of ~2.5-3.5s each, summing to exactly 8-10s. State shot timestamps accordingly (e.g., [Shot 1] 0-4s, [Shot 2] 4-9s).
5. Every shot must be composed and directed natively for 9:16 vertical orientation: frame subjects and action for a tall frame (no horizontal-letterbox blocking, no compositions that only work landscape), keep key action/text-safe area centered within the vertical frame.

## Step 4 — Language rules for the script itself
- `integrated_multimodal_description`: all visual/action/camera/lighting/setting description text must be written in English — this is what AI video models (Kling, Runway, Sora, Veo) parse most reliably for camera and action control. Use [Shot N] markers with timestamps for cuts after Shot 1, and describe camera motion as natural English action (type + amplitude + speed only when meaningful).
- Dialogue: use speaker IDs (S1)/(S2) with the tag `<d>[Vietnamese] ...</d>` — the actual spoken line inside the tag must be written in Vietnamese (the target audience's language), keeping the `[Vietnamese]` language marker as part of the tag format.
- On-screen text: wrap in `"..."` and write the text itself in Vietnamese, matching the target audience's language.
- `overall_soundscape`: 1-4 sentences in English, ambience/physical/non-verbal sound only (no dialogue, no music).
- `non_diegetic_music`: 1-3 sentences in English on instrumentation/tempo/dynamics (write `N/A` if none).

## Step 5 — Write the final video prompt (this is your ENTIRE visible output, verbatim)
Using the COMPRESSED 2-3 shot flow from Step 3 (not the pattern's original 4-6 shot SHOT_FLOW) as your narrative skeleton, and the pattern's CAMERA_NOTES/AUDIO_NOTES as your technical guide, output strictly in this order and nothing more:

integrated_multimodal_description: [Shot 1] ...

overall_soundscape: ...

non_diegetic_music: ...
```

## PART 5 — Gemini API Integration Example (Python)

```python
import os
from google import genai
from google.genai import types

# Model note (as of Aug 2026): Google's current frontier family is Gemini 3.x
# (3.1 Pro, 3.7 Flash — newest as of Aug 13 2026 — plus 3.6 Flash, 3.5 Flash,
# 3.5/3.1 Flash-Lite). Gemini 2.5 (Pro/Flash/Flash-Lite) remains available as a
# cheaper fallback tier. gemini-2.5-flash/gemini-2.5-pro are NOT on a hard
# deprecation path as of this writing but are the older generation.
# "gemini-flash-latest" is a Google-managed alias that always resolves to the
# current GA Flash model (currently Gemini 3.5 Flash GA per Google's release
# notes), so it needs less manual upkeep than pinning an exact version string.
# Pin an explicit version (e.g. "gemini-3.7-flash") instead if you need fully
# reproducible outputs across runs — check ai.google.dev/gemini-api/docs/changelog
# periodically since Google ships new Flash versions roughly every 4-8 weeks.
MODEL_NAME = "gemini-flash-latest"

client = genai.Client(api_key=os.environ["GEMINI_API_KEY"])

# Load the pattern library once (this file) and reuse across calls
with open("ad_video_pattern_library_context.md", "r", encoding="utf-8") as f:
    PATTERN_LIBRARY = f.read()

# Paste the Part 4 system instruction block here (the fenced block above)
SYSTEM_INSTRUCTION = """[paste the Part 4 system instruction block here]"""


def generate_ad_video_prompt(product_name: str, product_description: str,
                              target_audience: str, recently_used: list[str] = None) -> str:
    recently_used = recently_used or []
    user_message = f"""
Product name: {product_name}
Product description: {product_description}
Target audience: {target_audience}
Recently used patterns to avoid repeating: {', '.join(recently_used) if recently_used else 'none'}
"""

    response = client.models.generate_content(
        model=MODEL_NAME,
        contents=user_message,
        config=types.GenerateContentConfig(
            # Context file + instructions both go into system_instruction, not
            # into `contents` — this keeps the user turn clean (just the product
            # brief) and lets the model treat the library/rules as fixed ground
            # truth rather than conversational content it might paraphrase.
            system_instruction=[PATTERN_LIBRARY, SYSTEM_INSTRUCTION],
            temperature=0.7,
        ),
    )
    # Defensive strip: guards against stray leading/trailing whitespace or
    # newlines even though the system instruction forbids extra content.
    return response.text.strip()


# Example call
result = generate_ad_video_prompt(
    product_name="Bình giữ nhiệt XYZ",
    product_description="Bình giữ nhiệt 500ml, giữ lạnh 24h, thiết kế chống trượt, có quai xách",
    target_audience="Gen Z, năng động, hay tập gym và đi làm",
    recently_used=["unboxing_reveal", "macro_detail_texture"],
)
print(result)
```

## Gợi ý vận hành
- Lưu `recently_used` theo từng sản phẩm (ví dụ 3-5 pattern gần nhất trong database) để tránh lặp qua các lần generate liên tiếp cho cùng một khách hàng.
- Nếu muốn tiết kiệm token, có thể chỉ gửi các dòng PATTERN_ID có rating H cho category tương ứng thay vì toàn bộ 26 pattern — nhưng gửi cả file vẫn nằm rất xa giới hạn context window của các model Gemini hiện tại nên không bắt buộc phải cắt.
- Vì output giờ đã bị khoá cứng chỉ còn 3 field T2VA (không còn dòng "Selected pattern: ..."), nếu cần log lại pattern nào đã được chọn cho mục đích theo dõi/tránh lặp, nên gọi thêm một lần request phụ (hoặc dùng structured output/JSON riêng) thay vì cố lấy thông tin đó từ response chính.
- Định kỳ (1-3 tháng) chạy lại `research_prompt_library_builder.md` (đã tạo ở bước trước) để bổ sung pattern mới, rồi merge thủ công vào file này theo đúng cấu trúc PATTERN_ID hiện có.