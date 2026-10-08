// Add this JavaScript to your MuscleMap view or as a separate JS file

// Rank color mapping
const rankColors = {
    'Wood': '#8B4513',
    'Bronze': '#CD7F32',
    'Silver': '#C0C0C0',
    'Gold': '#FFD700',
    'Platinum': '#E5E4E2',
    'Emerald': '#50C878',
    'Diamond': '#B9F2FF',
    'Champion': '#FF0000'
};

// Function to get user's muscle rankings and apply colors
async function colorMusclesByRank() {
    try {
        // Fetch user's muscle rankings from backend
        const response = await fetch('/Home/GetUserMuscleRankings');
        const rankings = await response.json();
        
        // Apply colors to each muscle group
        rankings.forEach(ranking => {
            const muscleElement = document.querySelector(`[data-muscle-id="${ranking.muscleGroupId}"]`);
            if (muscleElement) {
                const rank = ranking.rank;
                const color = rankColors[rank] || '#666666'; // Default gray if no rank
                
                // Color all paths within the muscle group
                muscleElement.querySelectorAll('path').forEach(path => {
                    path.style.fill = color;
                });
                
                // Add rank badge/tooltip
                muscleElement.setAttribute('data-rank', rank);
                muscleElement.setAttribute('title', `${ranking.muscleName}: ${rank} (${ranking.maxWeight}kg)`);
            }
        });
        
        // Color unranked muscles with default gray
        document.querySelectorAll('.bodymap').forEach(muscle => {
            if (!muscle.hasAttribute('data-rank')) {
                muscle.querySelectorAll('path').forEach(path => {
                    path.style.fill = '#444444'; // Dark gray for unranked
                });
                const muscleName = muscle.getAttribute('data-muscle-name');
                muscle.setAttribute('title', `${muscleName}: Not ranked yet`);
            }
        });
        
    } catch (error) {
        console.error('Error loading muscle rankings:', error);
    }
}

// Alternative: Inline the data if you're rendering in Razor
function colorMusclesByRankInline(rankings) {
    rankings.forEach(ranking => {
        const muscleElement = document.querySelector(`[data-muscle-id="${ranking.muscleGroupId}"]`);
        if (muscleElement) {
            const rank = ranking.rank;
            const color = rankColors[rank] || '#666666';
            
            muscleElement.querySelectorAll('path').forEach(path => {
                path.style.fill = color;
            });
            
            muscleElement.setAttribute('data-rank', rank);
            muscleElement.setAttribute('title', `${ranking.muscleName}: ${rank} (${ranking.maxWeight}kg)`);
        }
    });
    
    // Color unranked muscles
    document.querySelectorAll('.bodymap').forEach(muscle => {
        if (!muscle.hasAttribute('data-rank')) {
            muscle.querySelectorAll('path').forEach(path => {
                path.style.fill = '#444444';
            });
            const muscleName = muscle.getAttribute('data-muscle-name');
            muscle.setAttribute('title', `${muscleName}: Not ranked yet`);
        }
    });
}

// Call on page load
document.addEventListener('DOMContentLoaded', function() {
    colorMusclesByRank();
});

// Add CSS for hover effects
const style = document.createElement('style');
style.textContent = `
    .bodymap {
        cursor: pointer;
        transition: opacity 0.3s ease;
    }
    
    .bodymap:hover {
        opacity: 0.8;
        filter: brightness(1.2);
    }
    
    .bodymap path {
        transition: fill 0.3s ease;
    }
`;
document.head.appendChild(style);
