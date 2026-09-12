# E2E media fixture

`product-demo.webm` is a generated one-second, 32×32 blue video (25 fps, VP8, no audio). It contains no third-party footage or user data. The fixture replaces canvas/MediaRecorder generation, which intermittently produced unplayable WebM in headless Edge.

Generated locally with 25 blue JPEG frames encoded by Playwright FFmpeg 1011 using `image2pipe`, `mjpeg`, `libvpx`, and `yuv420p`. Tests load the committed bytes directly; FFmpeg is not required to generate this fixture at test time (Playwright still needs it for recording failure videos).
