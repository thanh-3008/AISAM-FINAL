const { Client } = require('pg');

const client = new Client({
  connectionString: 'postgres://postgres.dhzdvcnepphpjuwsyook:5a704b07!10e0@aws-1-ap-northeast-1.pooler.supabase.com/postgres',
  ssl: {
    rejectUnauthorized: false
  }
});

async function main() {
  await client.connect();
  const res = await client.query("SELECT email, role, full_name FROM users WHERE email = 'admin@aisam.ai'");
  console.log('Query result:', res.rows);
  await client.end();
}

main().catch(err => console.error(err));
