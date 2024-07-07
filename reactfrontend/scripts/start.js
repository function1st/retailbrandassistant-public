const readline = require('readline');
const { spawn } = require('child_process');

const rl = readline.createInterface({
  input: process.stdin,
  output: process.stdout
});

rl.question('Turn Off System Messages in the UX? (y/N) ', (answer) => {
  const hideSystemMessages = answer.toLowerCase() === 'y';
  console.log(`System messages will be ${hideSystemMessages ? 'hidden' : 'shown'}`);
  
  const env = { ...process.env, REACT_APP_HIDE_SYSTEM_MESSAGES: hideSystemMessages };
  
  const child = spawn('react-scripts', ['start'], { 
    stdio: 'inherit',
    env: env
  });

  child.on('exit', (code) => {
    process.exit(code);
  });

  rl.close();
});