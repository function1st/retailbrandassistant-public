const { spawn } = require('child_process');

const hideSystemMessages = true;
console.log(`System messages will be ${hideSystemMessages ? 'hidden' : 'shown'}`);

const env = { ...process.env, REACT_APP_HIDE_SYSTEM_MESSAGES: hideSystemMessages };

const child = spawn('react-scripts', ['start'], { 
  stdio: 'inherit',
  env: env
});

child.on('exit', (code) => {
  process.exit(code);
});
