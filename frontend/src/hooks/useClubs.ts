import { useQuery } from '@tanstack/react-query';
import { getClubs } from '../api';
import useLocalStorage from './useLocalStorage';

export default function useClubs() {
    const [selectedClubId, setSelectedClubId] = useLocalStorage<number | null>("club", null);
    const { data: clubs, refetch: refetchClubs, error } = useQuery({
        queryKey: ['club-list'],
        queryFn: getClubs,
    });
    const clubId = selectedClubId ? selectedClubId : (clubs && clubs.length > 0 ? clubs[0].id : 0);

    const activeClub = clubs?.find(club => club.id === clubId);

    return { clubs, clubId, refetchClubs, setSelectedClubId, error, activeClub }
}