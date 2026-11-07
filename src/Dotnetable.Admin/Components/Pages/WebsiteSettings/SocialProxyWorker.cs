namespace Dotnetable.Admin.Components.Pages.WebsiteSettings;

/// <summary>Worker the admin pastes into Cloudflare. The app calls /proxy/b64/{base64url}.</summary>
public static class SocialProxyWorker
{
    public const string Source = """
        // Put the same key you save in Dotnetable here. Leave both empty to skip the check.
        const SECRET = "";

        export default {
          async fetch(request) {
            const url = new URL(request.url);
            const prefix = "/proxy/b64/";
            if (!url.pathname.startsWith(prefix)) {
              return new Response("Not found", { status: 404 });
            }

            const given = url.searchParams.get("secret") || request.headers.get("x-proxy-key") || "";
            if (SECRET && given !== SECRET) {
              return new Response("Unauthorized", { status: 401 });
            }

            let target;
            try {
              target = decodeB64Url(url.pathname.slice(prefix.length));
            } catch {
              return new Response("Bad target", { status: 400 });
            }
            if (!target.startsWith("https://")) {
              return new Response("Target must be https", { status: 400 });
            }

            const headers = new Headers(request.headers);
            ["cf-connecting-ip", "x-forwarded-for", "x-real-ip", "cf-ray", "cf-ipcountry", "cf-visitor", "host", "x-proxy-key", "content-length", "transfer-encoding", "connection", "keep-alive", "te", "trailer", "upgrade", "expect"]
              .forEach((name) => headers.delete(name));

            // A streamed body makes the worker throw before Telegram is called.
            const method = request.method.toUpperCase();
            const body = method === "GET" || method === "HEAD" ? undefined : await request.arrayBuffer();

            try {
              const response = await fetch(target, {
                method,
                headers,
                body: body && body.byteLength > 0 ? body : undefined,
                redirect: "follow",
              });
              const outHeaders = new Headers(response.headers);
              outHeaders.set("access-control-allow-origin", "*");
              return new Response(response.body, {
                status: response.status,
                statusText: response.statusText,
                headers: outHeaders,
              });
            } catch (err) {
              return new Response(JSON.stringify({ error: String(err && err.message || err) }), {
                status: 502,
                headers: { "content-type": "application/json" },
              });
            }
          },
        };

        function decodeB64Url(value) {
          const pad = (4 - (value.length % 4)) % 4;
          const padded = value.replace(/-/g, "+").replace(/_/g, "/") + "=".repeat(pad);
          const binary = atob(padded);
          const bytes = Uint8Array.from(binary, (ch) => ch.charCodeAt(0));
          return new TextDecoder().decode(bytes);
        }
        """;
}
