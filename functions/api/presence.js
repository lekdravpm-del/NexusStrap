const presence = new Map(); // deviceId -> lastSeen ms
const TTL_MS = 90 * 1000; // 90s consider offline

function clean() {
  const now = Date.now();
  for (const [id, ts] of presence.entries()) {
    if (now - ts > TTL_MS) presence.delete(id);
  }
}

export async function onRequest(context) {
  const { request } = context;
  clean();

  const headers = {
    "Access-Control-Allow-Origin": "*",
    "Access-Control-Allow-Methods": "GET, POST, OPTIONS",
    "Access-Control-Allow-Headers": "Content-Type",
    "Content-Type": "application/json"
  };

  if (request.method === "OPTIONS") {
    return new Response(null, { headers });
  }

  if (request.method === "POST") {
    try {
      const body = await request.json();
      const id = body.deviceId || body.device_id;
      if (id) {
        presence.set(id, Date.now());
        // also track total unique ever seen via KV if available, else use size
        clean();
        return new Response(JSON.stringify({ ok: true, online: presence.size }), { headers });
      }
    } catch {}
    return new Response(JSON.stringify({ ok: false }), { status: 400, headers });
  }

  // GET -> return online count and total (total = online for now, could be KV total)
  clean();
  const online = presence.size || 1; // at least 1 for demo single user
  return new Response(JSON.stringify({ online, total: online, inactive: 0 }), { headers });
}
