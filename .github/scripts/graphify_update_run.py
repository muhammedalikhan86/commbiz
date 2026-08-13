"""
Graphify update run for F-014 — Self-balancing detail record + minimum batch size reduction
Scoped to src/ directory only
"""
import json
import sys
from pathlib import Path
from graphify.detect import detect
from graphify.extract import collect_files, extract
from graphify.build import build_from_json
from graphify.cluster import cluster, score_all
from graphify.analyze import god_nodes, surprising_connections, suggest_questions
from graphify.report import generate
from graphify.export import to_json

# Ensure output directory
Path('graphify-out').mkdir(exist_ok=True)

print("\n=== GRAPHIFY UPDATE RUN ===")
print("Scope: src/ directory (F-014)")
print()

# Step 1: Detect files in src/
print("Step 1: Detecting files in src/...")
detect_result = detect(Path('src'))
print(f"  Code: {len(detect_result['files'].get('code', []))} files")
print(f"  Docs: {len(detect_result['files'].get('docs', []))} files")
print(f"  Total: {detect_result['total_files']} files, ~{detect_result['total_words']} words")
print()

# Step 2: AST extraction for code files
print("Step 2: Extracting code structure (AST)...")
code_files = []
for f in detect_result.get('files', {}).get('code', []):
    if Path(f).is_file():
        code_files.append(Path(f))
    elif Path(f).is_dir():
        code_files.extend(Path(f).glob('**/*'))

code_files = [f for f in code_files if f.is_file()]
print(f"  Collecting {len(code_files)} code files...")

if code_files:
    ast_result = extract(code_files)
    print(f"  ✓ AST: {len(ast_result['nodes'])} nodes, {len(ast_result['edges'])} edges")
    Path('graphify-out/.graphify_ast.json').write_text(
        json.dumps(ast_result, indent=2, ensure_ascii=False), 
        encoding="utf-8"
    )
else:
    print("  No code files found")
    ast_result = {'nodes': [], 'edges': [], 'input_tokens': 0, 'output_tokens': 0}
    Path('graphify-out/.graphify_ast.json').write_text(
        json.dumps(ast_result, ensure_ascii=False), 
        encoding="utf-8"
    )

print()

# Step 3: Load existing graph if present, merge with new extraction
print("Step 3: Merging with existing graph...")
existing_graph_path = Path('graphify-out/graph.json')
if existing_graph_path.exists():
    existing_data = json.loads(existing_graph_path.read_text(encoding="utf-8"))
    existing_nodes_count = len(existing_data.get('nodes', []))
    existing_edges_count = len(existing_data.get('edges', []))
    print(f"  Existing graph: {existing_nodes_count} nodes, {existing_edges_count} edges")
    
    # Merge: keep existing, add new AST nodes that don't exist
    existing_ids = {n['id'] for n in existing_data.get('nodes', [])}
    new_ast_nodes = [n for n in ast_result.get('nodes', []) if n['id'] not in existing_ids]
    
    merged_nodes = existing_data.get('nodes', []) + new_ast_nodes
    merged_edges = existing_data.get('edges', []) + ast_result.get('edges', [])
    merged_extraction = {
        'nodes': merged_nodes,
        'edges': merged_edges,
        'input_tokens': ast_result.get('input_tokens', 0),
        'output_tokens': ast_result.get('output_tokens', 0)
    }
    print(f"  ✓ Merged: {len(new_ast_nodes)} new nodes added")
    print(f"  Total now: {len(merged_nodes)} nodes, {len(merged_edges)} edges")
else:
    print("  No existing graph - using fresh extraction")
    merged_extraction = ast_result

print()

# Step 4: Build graph with clustering and analysis
print("Step 4: Building graph with clustering and community detection...")
if len(merged_extraction['nodes']) == 0:
    print("  ERROR: No nodes to build graph from")
    sys.exit(1)

G = build_from_json(merged_extraction)
communities = cluster(G)
cohesion = score_all(G, communities)
gods = god_nodes(G)
surprises = surprising_connections(G, communities)
labels = {cid: f'Community {cid}' for cid in communities}
questions = suggest_questions(G, communities, labels)

print(f"  ✓ Graph: {G.number_of_nodes()} nodes, {G.number_of_edges()} edges")
print(f"  ✓ Communities: {len(communities)} detected")
print()

# Step 5: Generate report and export
print("Step 5: Generating report and exporting...")
report = generate(
    G, communities, cohesion, labels, gods, surprises, 
    detect_result,
    {'input': merged_extraction.get('input_tokens', 0), 'output': merged_extraction.get('output_tokens', 0)},
    'src'
)
Path('graphify-out/GRAPH_REPORT.md').write_text(report, encoding="utf-8")
print("  ✓ GRAPH_REPORT.md written")

to_json(G, communities, 'graphify-out/graph.json')
print("  ✓ graph.json written")

print()
print("=== UPDATE COMPLETE ===")
print(f"Output directory: graphify-out/")
print(f"  - graph.json (updated knowledge graph)")
print(f"  - GRAPH_REPORT.md (audit report)")
print()
