# BloodCraftPlus Documentation Site

This folder contains the source for the BloodCraftPlus documentation website, built with Vite + React + TypeScript.

## Prerequisites

- **Node.js 20+** (LTS recommended)
- **npm** (comes with Node.js)

## Quick Start

```bash
# Install dependencies
npm install

# Start development server
npm run dev

# Open http://localhost:5173 in your browser
```

## Commands

| Command | Description |
|---------|-------------|
| `npm run dev` | Start development server with hot reload |
| `npm run build` | Build for production |
| `npm run preview` | Preview production build locally |
| `npm run generate` | Generate reference docs from source |

## Project Structure

```
Docs/
├── public/               # Static assets
│   └── images/
│       └── banner.png    # Site banner
├── src/
│   ├── main.tsx          # App entry point
│   ├── App.tsx           # Route definitions
│   ├── docs.config.ts    # Navigation configuration
│   ├── components/       # Reusable UI components
│   │   ├── Layout.tsx    # Page layout with sidebar
│   │   ├── Callout.tsx   # Alert/info boxes
│   │   ├── CodeBlock.tsx # Syntax-highlighted code
│   │   ├── CommandCard.tsx
│   │   └── ConfigTable.tsx
│   ├── content/          # MDX documentation pages
│   │   ├── index.mdx     # Home page
│   │   ├── getting-started/
│   │   ├── server/
│   │   ├── client/
│   │   ├── reference/
│   │   ├── tools/
│   │   └── contributing/
│   └── styles/
│       ├── tokens.css    # Design tokens (colors, spacing)
│       └── globals.css   # Global styles
├── scripts/
│   └── generate-reference.ts  # Config reference generator
├── package.json
├── vite.config.ts
└── tsconfig.json
```

## Adding a New Page

1. **Create the MDX file** in the appropriate `src/content/` folder:

   ```mdx
   # Page Title

   <Callout type="info">
     This is a callout box.
   </Callout>

   ## Section

   Content goes here...
   ```

2. **Add the route** in `src/App.tsx`:

   ```tsx
   import NewPage from './content/section/new-page.mdx';
   
   // In Routes:
   <Route path="/section/new-page" element={<NewPage />} />
   ```

3. **Add to navigation** in `src/docs.config.ts`:

   ```ts
   {
     title: 'Section',
     children: [
       { title: 'New Page', path: '/section/new-page' },
     ],
   }
   ```

## Using Components in MDX

Components are automatically available in MDX files:

### Callout

```mdx
<Callout type="info">Information message</Callout>
<Callout type="warning">Warning message</Callout>
<Callout type="danger">Danger message</Callout>
<Callout type="tip">Helpful tip</Callout>
```

### Code Blocks

````mdx
```typescript
const example = "syntax highlighted";
```
````

### Tables

Standard markdown tables work:

```mdx
| Column 1 | Column 2 |
|----------|----------|
| Value 1  | Value 2  |
```

## Styling

### Design Tokens

All colors, spacing, and typography are defined in `src/styles/tokens.css`:

```css
:root {
  --bg0: #0d0d10;      /* Background */
  --accent: #c41e3a;   /* Primary accent */
  --fg0: #f4f0ec;      /* Text color */
  /* ... */
}
```

### Theme: "Dracula Vibes"

The site uses a dark, vampiric theme with:
- Dark backgrounds (`--bg0`, `--bg1`, `--bg2`)
- Blood red accent (`--accent`)
- Gothic typography (Cinzel for headings)
- Dracula-ish syntax highlighting

## Deployment

### GitHub Pages (Automatic)

The site deploys automatically via GitHub Actions:
- **Push to `main`** → Builds and deploys to GitHub Pages
- **Pull requests** → Builds only (no deploy)

Workflow: `.github/workflows/docs.yml`

### Manual Deploy

```bash
npm run build
# Upload contents of dist/ to your host
```

### Base Path

The Vite config automatically sets the base path for GitHub Pages:
- Default: `/${repoName}/`
- Custom domain: Set `VITE_BASE_PATH=/` environment variable

## Reference Generation

The `generate` script parses source files to create documentation:

```bash
npm run generate
```

This reads `Server/Bloodcraftplus/Services/ConfigService.cs` and generates
`src/content/reference/config-generated.mdx`.

Note: The main `config.mdx` is manually maintained for better formatting.
The generated file serves as a reference for keeping docs in sync.

## Contributing

See the [Contributing Guide](/contributing) on the live site or
`src/content/contributing/index.mdx` in this repo.
