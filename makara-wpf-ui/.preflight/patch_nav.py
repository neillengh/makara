import re, os
nav_map = {
    'dashboard': 'nav-dashboard',
    'workflows': 'nav-workflows',
    'data-sources': 'nav-data-sources',
    'datasets': 'nav-datasets',
    'runs': 'nav-runs',
    'servers': 'nav-servers',
    'settings': 'nav-settings',
}
base = r'e:\WorkSpace\Trae WorkSpace\openname\makara-wpf-ui'
for folder in [os.path.join(base, 'pages'), os.path.join(base, 'partials')]:
    if not os.path.isdir(folder):
        continue
    for name in os.listdir(folder):
        if not name.endswith('.html'):
            continue
        path = os.path.join(folder, name)
        with open(path, 'r', encoding='utf-8') as f:
            content = f.read()
        for key, dom_id in nav_map.items():
            pattern = rf'data-nav-key="{re.escape(key)}"'
            repl = f'data-dom-id="{dom_id}" data-nav-key="{key}"'
            content = re.sub(pattern, repl, content)
        with open(path, 'w', encoding='utf-8') as f:
            f.write(content)
        print('patched', path)