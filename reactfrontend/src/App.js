import React, { useState, useEffect, useRef } from 'react';
import { HubConnectionBuilder, HubConnectionState, HttpTransportType } from '@microsoft/signalr';
import MarkdownIt from 'markdown-it';
import './App.css';

const md = new MarkdownIt();

const Spinner = () => (
  <div className="spinner">
    <div className="bounce1"></div>
    <div className="bounce2"></div>
    <div className="bounce3"></div>
  </div>
);

const hideSystemMessages = process.env.REACT_APP_HIDE_SYSTEM_MESSAGES === 'true';

function MetricsDisplay({ metrics, currentTopic }) {
  return (
    <div className="metrics-display">
      <h3>Chat Metrics</h3>
      <ul>
        <li>TTFT: {metrics.ttft} ms</li>
        <li>End-to-End Duration: {metrics.endToEndDuration} ms</li>
        <li>Function Duration: {metrics.functionDuration} ms</li>
        <li>Context Character Length: {metrics.contextCharacterLength}</li>
        <li>Total Input Tokens: {metrics.totalInputTokens}</li>
        <li>Total Output Tokens: {metrics.totalOutputTokens}</li>
        <li>Total Parallel Operation Time: {metrics.totalParallelOperationTime} ms</li>
        <li>Total Parallel Operations: {metrics.totalParallelOperations}</li>
        <li>Average Parallel Operation Time: {metrics.averageParallelOperationTime} ms</li>
      </ul>
      <h3>Current Topic</h3>
      <div className="current-topic">{currentTopic || 'No topic set'}</div>
    </div>
  );
}

function App() {
  const [connection, setConnection] = useState(null);
  const [chat, setChat] = useState([]);
  const [message, setMessage] = useState('');
  const [brand, setBrand] = useState('');
  const [connecting, setConnecting] = useState(true);
  const [error, setError] = useState(null);
  const [isGenerating, setIsGenerating] = useState(false);
  const [generatingMessage, setGeneratingMessage] = useState('');
  const [currentTopic, setCurrentTopic] = useState('');
  const [metrics, setMetrics] = useState({
    ttft: 0,
    endToEndDuration: 0,
    functionDuration: 0,
    contextCharacterLength: 0,
    totalInputTokens: 0,
    totalOutputTokens: 0,
    totalParallelOperationTime: 0,
    totalParallelOperations: 0,
    averageParallelOperationTime: 0
  });
  const latestChat = useRef(null);
  const messagesEndRef = useRef(null);

  latestChat.current = chat;

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  };

  useEffect(scrollToBottom, [chat, generatingMessage]);

  const appendSystemMessage = (prevChat, newMessage) => {
    if (hideSystemMessages) {
      return prevChat;
    }
    const updatedChat = [...prevChat];
    const lastMessage = updatedChat[updatedChat.length - 1];
    
    if (lastMessage && lastMessage.role === 'system') {
      if (!lastMessage.content.includes(newMessage)) {
        lastMessage.content += '\n' + newMessage;
      }
      return updatedChat;
    } else {
      return [...updatedChat, { role: 'system', content: newMessage }];
    }
  };

  useEffect(() => {
    const newConnection = new HubConnectionBuilder()
      .withUrl('http://localhost:5000/chatHub', {
        skipNegotiation: true,
        transport: HttpTransportType.WebSockets
      })
      .withAutomaticReconnect()
      .build();
  
    setConnection(newConnection);
  
    const startConnection = async () => {
      try {
        await newConnection.start();
        console.log('Connected to SignalR hub');
        setConnecting(false);
  
        newConnection.on('ReceiveInitialMessage', message => {
          console.log('Received initial message:', message);
          setChat([{ role: 'assistant', content: message }]);
          setIsGenerating(false);
          setGeneratingMessage('');
        });
  
        newConnection.on('ReceiveMessage', message => {
          console.log('Received message:', message);
          setChat(prevChat => [...prevChat, { role: 'assistant', content: message }]);
          setIsGenerating(false);
          setGeneratingMessage('');
        });

        newConnection.on('ReceiveMessageStream', message => {
          console.log('Received message stream:', message);
          setChat(prevChat => {
            const updatedChat = [...prevChat];
            if (updatedChat.length > 0 && updatedChat[updatedChat.length - 1].role === 'assistant') {
              updatedChat[updatedChat.length - 1] = {
                ...updatedChat[updatedChat.length - 1],
                content: updatedChat[updatedChat.length - 1].content + message
              };
            } else {
              updatedChat.push({ role: 'assistant', content: message });
            }
            return updatedChat;
          });
          setIsGenerating(false);
          setGeneratingMessage('');
        });

        newConnection.on('ReceiveSystemMessage', message => {
          console.log('Received system message:', message);
          setGeneratingMessage(prev => prev ? `${prev}\n${message}` : `Generating...\n${message}`);
          if (!hideSystemMessages) {
            setChat(prevChat => appendSystemMessage(prevChat, message));
          }
          if (message.startsWith('Current topic:')) {
            setCurrentTopic(message.replace('Current topic:', '').trim());
          }
        });

        newConnection.on('UpdateMetrics', updatedMetrics => {
          console.log('Received updated metrics:', updatedMetrics);
          setMetrics(updatedMetrics);
        });

        await newConnection.invoke('InitializeChat');
      } catch (e) {
        console.error('Connection failed:', e);
        setConnecting(false);
        setError('Failed to connect to the server. Please try again.');
      }
    };
  
    startConnection();

    return () => {
      if (newConnection && newConnection.state === HubConnectionState.Connected) {
        newConnection.stop();
      }
    };
  }, []);

  useEffect(() => {
    let isFetching = false;

    const fetchBrand = async () => {
      if (isFetching) return;
      isFetching = true;
      console.log('Fetching brand...');
      try {
        const response = await fetch('http://localhost:5000/api/brand', {
          credentials: 'include'
        });
        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }
        const data = await response.json();
        setBrand(data.brand);
        document.title = `${data.brand}: Retail Brand Assistant`;
      } catch (error) {
        console.error('Error fetching brand:', error);
        setError(`Failed to fetch brand information. ${error.message}`);
      } finally {
        isFetching = false;
      }
    };

    fetchBrand();
  }, []);

  const sendMessage = async () => {
    if (connection && message.trim()) {
      try {
        console.log('Sending message:', message);
        setChat(prevChat => [...prevChat, { role: 'user', content: message }]);
        setMessage('');
        setIsGenerating(true);
        setGeneratingMessage('Generating...');
        await connection.invoke('SendMessage', message);
      } catch (e) {
        console.error('Send message failed:', e);
        setError('Failed to send message. Please try again.');
        setIsGenerating(false);
        setGeneratingMessage('');
      }
    }
  }

  if (error) {
    return (
      <div className="error-container">
        <p>An error occurred: {error}</p>
        <button onClick={() => window.location.reload()}>Retry</button>
      </div>
    );
  }

  if (connecting) {
    return (
      <div className="connecting-container">
        <p>Connecting to the chat server... Please wait.</p>
      </div>
    );
  }

  return (
    <div className="app-container">
      <div className="brand-page-placeholder">
        <h2>{brand ? `${brand} Website Here` : 'Brand Page Here'}</h2>
      </div>
      <div className="chat-app">
        <header className="chat-header">
          <h1>{brand} Retail Brand Assistant</h1>
        </header>
        <div className="chat-container">
          <div className="message-list">
            {chat.map((m, index) => (
              <div key={index} className={`message ${m.role}`}>
                {m.role !== 'system' && (
                  <div className="avatar">
                    {m.role === 'assistant' ? 'AI' : 'USER'}
                  </div>
                )}
                {(m.role !== 'system' || !hideSystemMessages) && (
                  m.role === 'system' ? (
                    <div className="system-message-content">
                      {m.content.split('\n').map((line, i) => (
                        <div key={i} dangerouslySetInnerHTML={{ __html: md.render(line) }} />
                      ))}
                    </div>
                  ) : (
                    <div className={`message-content ${m.role}`} dangerouslySetInnerHTML={{ __html: md.render(m.content) }} />
                  )
                )}
              </div>
            ))}
            {isGenerating && (
              <div className="message assistant">
                <div className="avatar">AI</div>
                <div className="message-content assistant">
                  {generatingMessage.split('\n').map((line, i) => (
                    <div key={i} className={i === 0 ? 'generating-text' : 'system-update-text'}>
                      {line}
                    </div>
                  ))}
                  <Spinner />
                </div>
              </div>
            )}
            <div ref={messagesEndRef} />
          </div>
        </div>
        <div className="input-area">
          <input
            type="text"
            value={message}
            onChange={(e) => setMessage(e.target.value)}
            onKeyPress={(e) => e.key === 'Enter' && sendMessage()}
            placeholder="Type your message here..."
          />
          <button onClick={sendMessage}>Send</button>
        </div>
        <MetricsDisplay metrics={metrics} currentTopic={currentTopic} />
      </div>
    </div>
  );
}

export default App;
