import type { MDXComponents } from 'mdx/types';
import { useLocation } from 'react-router-dom';
import { CodeBlock } from './CodeBlock';
import { Callout } from './Callout';
import { PortraitShotPlaceholder } from './PortraitShotPlaceholder';
import { ChangelogSection } from './ChangelogSection';

/**
 * Custom MDX components that override default markdown rendering
 */
export const mdxComponents: MDXComponents = {
  // Override pre/code for syntax highlighting
  pre: ({ children, ...props }) => {
    // Extract code element and its props
    const codeElement = children as React.ReactElement;
    if (codeElement?.props?.className) {
      const language = codeElement.props.className.replace('language-', '');
      return (
        <CodeBlock language={language} {...props}>
          {codeElement.props.children}
        </CodeBlock>
      );
    }
    return <pre {...props}>{children}</pre>;
  },

  // Custom components available in MDX
  Callout,
  CodeBlock,
  PortraitShotPlaceholder,
  ChangelogSection,

  // Style overrides for headings with anchor links
  h2: ({ children, id, ...props }) => {
    const location = useLocation();
    const isChangelogPage = location.pathname === '/reference/changelog';

    // Don’t make date/version headings clickable on the changelog page.
    if (isChangelogPage || !id) {
      return (
        <h2 id={id} {...props}>
          {children}
        </h2>
      );
    }

    return (
      <h2 id={id} {...props}>
        <a href={`#${id}`} className="anchor-link">
          {children}
        </a>
      </h2>
    );
  },
  h3: ({ children, id, ...props }) => {
    const location = useLocation();
    const isChangelogPage = location.pathname === '/reference/changelog';

    if (isChangelogPage || !id) {
      return (
        <h3 id={id} {...props}>
          {children}
        </h3>
      );
    }

    return (
      <h3 id={id} {...props}>
        <a href={`#${id}`} className="anchor-link">
          {children}
        </a>
      </h3>
    );
  },
};
