import { Highlight, themes } from 'prism-react-renderer';
import { useState } from 'react';
import './CodeBlock.css';

interface CodeBlockProps {
  children: string;
  language?: string;
  title?: string;
}

export function CodeBlock({ children, language = 'text', title }: CodeBlockProps) {
  const [copied, setCopied] = useState(false);
  const code = children.trim();

  const handleCopy = async () => {
    await navigator.clipboard.writeText(code);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  return (
    <div className="code-block">
      <div className="code-header">
        {title && <span className="code-title">{title}</span>}
        <span className="code-language">{language}</span>
        <button className="code-copy" onClick={handleCopy}>
          {copied ? 'Copied!' : 'Copy'}
        </button>
      </div>
      <Highlight theme={draculaTheme} code={code} language={language}>
        {({ className, style, tokens, getLineProps, getTokenProps }) => (
          <pre className={className} style={style}>
            {tokens.map((line, i) => (
              <div key={i} {...getLineProps({ line })}>
                <span className="line-number">{i + 1}</span>
                {line.map((token, key) => (
                  <span key={key} {...getTokenProps({ token })} />
                ))}
              </div>
            ))}
          </pre>
        )}
      </Highlight>
    </div>
  );
}

// Custom Dracula-inspired theme
const draculaTheme = {
  ...themes.dracula,
  plain: {
    color: '#f8f8f2',
    backgroundColor: '#14141a',
  },
  styles: [
    ...themes.dracula.styles,
    {
      types: ['comment', 'prolog', 'doctype', 'cdata'],
      style: { color: '#6272a4' },
    },
    {
      types: ['punctuation'],
      style: { color: '#f8f8f2' },
    },
    {
      types: ['property', 'tag', 'constant', 'symbol', 'deleted'],
      style: { color: '#ff79c6' },
    },
    {
      types: ['boolean', 'number'],
      style: { color: '#bd93f9' },
    },
    {
      types: ['selector', 'attr-name', 'string', 'char', 'builtin', 'inserted'],
      style: { color: '#50fa7b' },
    },
    {
      types: ['operator', 'entity', 'url', 'variable'],
      style: { color: '#f8f8f2' },
    },
    {
      types: ['atrule', 'attr-value', 'function', 'class-name'],
      style: { color: '#f1fa8c' },
    },
    {
      types: ['keyword'],
      style: { color: '#8be9fd' },
    },
    {
      types: ['regex', 'important'],
      style: { color: '#ffb86c' },
    },
  ],
};
