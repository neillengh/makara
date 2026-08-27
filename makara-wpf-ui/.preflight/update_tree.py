import json, os
base = r'e:\WorkSpace\Trae WorkSpace\openname\makara-wpf-ui'

with open(os.path.join(base, 'generation-tree.json'), 'r', encoding='utf-8') as f:
    tree = json.load(f)

def mark_generated(node):
    if node.get('kind') == 'page-leaf':
        node['status'] = 'generated'
    for child in node.get('children', []):
        mark_generated(child)

mark_generated(tree)

with open(os.path.join(base, 'generation-tree.json'), 'w', encoding='utf-8') as f:
    json.dump(tree, f, ensure_ascii=False, indent=2)

with open(os.path.join(base, 'runtime-orchestration-summary.json'), 'r', encoding='utf-8') as f:
    summary = json.load(f)

mark_generated(summary['project']['generationTree'])

with open(os.path.join(base, 'runtime-orchestration-summary.json'), 'w', encoding='utf-8') as f:
    json.dump(summary, f, ensure_ascii=False, indent=2)

print('tree statuses updated')