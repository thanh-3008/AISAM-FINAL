# AISAM API media upload limit

The production API is served through Nginx. Nginx defaults `client_max_body_size`
to 1 MiB, which rejects normal videos with HTTP 413 before ASP.NET Core can run
its application-level validation.

Include `aisam-api-upload-limit.conf` inside the existing
`server_name api.aisam.io.vn` server block:

```nginx
server {
    server_name api.aisam.io.vn;

    include /path/to/AISAM/deploy/nginx/aisam-api-upload-limit.conf;

    # Keep the existing TLS and proxy configuration unchanged.
}
```

Validate and reload after deployment:

```bash
sudo nginx -t
sudo systemctl reload nginx
```

The proxy allowance is intentionally 55 MiB rather than unbounded. The media
endpoint accepts multipart requests up to 55 MiB and rejects an individual
image or video larger than 50 MiB with a JSON application error. Other ASP.NET
Core endpoints retain their existing request limits.

Production verification must confirm that the include is active in the loaded
Nginx configuration (`sudo nginx -T`) and that a media upload larger than 1 MiB
now reaches `/api/content/media`.
