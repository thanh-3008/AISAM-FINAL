const { Client } = require("pg");
(async () => {
  const client = new Client({
    host: "aws-1-ap-northeast-1.pooler.supabase.com",
    database: "postgres",
    user: "postgres.dhzdvcnepphpjuwsyook",
    password: "5a704b07!10e0",
    ssl: { rejectUnauthorized: false }
  });
  await client.connect();
  const res = await client.query("SELECT id, email, role FROM users WHERE email = 'admin@aisam.ai'");
  console.log("Before:", JSON.stringify(res.rows));
  if (res.rows.length > 0) {
    await client.query("UPDATE users SET role = 2 WHERE email = 'admin@aisam.ai'");
    const verify = await client.query("SELECT id, email, role FROM users WHERE email = 'admin@aisam.ai'");
    console.log("After:", JSON.stringify(verify.rows));
  } else {
    console.log("User not found in DB - need to register first");
  }
  await client.end();
  console.log("Done");
})();
